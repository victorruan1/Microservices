using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ShippingDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "ShippingService" }));

app.MapGet(
    "/shipments",
    async (ShippingDbContext db) => await db.Shipments.AsNoTracking().ToListAsync()
);
app.MapPost(
    "/shipments",
    async (ShippingDbContext db, Shipment s) =>
    {
        db.Shipments.Add(s);
        await db.SaveChangesAsync();
        return Results.Created($"/shipments/{s.Id}", s);
    }
);

app.Run();

class Shipment
{
    public int Id { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

class ShippingDbContext : DbContext
{
    public ShippingDbContext(DbContextOptions<ShippingDbContext> options)
        : base(options) { }

    public DbSet<Shipment> Shipments => Set<Shipment>();
}
