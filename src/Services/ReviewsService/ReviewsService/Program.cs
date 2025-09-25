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
builder.Services.AddDbContext<ReviewsDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "ReviewsService" }));

app.MapGet(
    "/reviews",
    async (ReviewsDbContext db) => await db.Reviews.AsNoTracking().ToListAsync()
);
app.MapPost(
    "/reviews",
    async (ReviewsDbContext db, Review r) =>
    {
        db.Reviews.Add(r);
        await db.SaveChangesAsync();
        return Results.Created($"/reviews/{r.Id}", r);
    }
);

app.Run();

class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

class ReviewsDbContext : DbContext
{
    public ReviewsDbContext(DbContextOptions<ReviewsDbContext> options)
        : base(options) { }

    public DbSet<Review> Reviews => Set<Review>();
}
