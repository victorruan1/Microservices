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
builder.Services.AddDbContext<PromotionsDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "PromotionsService" }));

app.MapGet(
    "/promotions",
    async (PromotionsDbContext db) => await db.Promotions.AsNoTracking().ToListAsync()
);
app.MapPost(
    "/promotions",
    async (PromotionsDbContext db, Promotion p) =>
    {
        db.Promotions.Add(p);
        await db.SaveChangesAsync();
        return Results.Created($"/promotions/{p.Id}", p);
    }
);

app.Run();

class Promotion
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; }
    public DateTime ValidUntil { get; set; }
}

class PromotionsDbContext : DbContext
{
    public PromotionsDbContext(DbContextOptions<PromotionsDbContext> options)
        : base(options) { }

    public DbSet<Promotion> Promotions => Set<Promotion>();
}
