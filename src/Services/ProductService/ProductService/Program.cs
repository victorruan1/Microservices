using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration (reads from appsettings if present)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);
builder.Services.AddControllers();

var app = builder.Build();

// Run migrations & seed (dev only convenience)
if (app.Environment.IsDevelopment())
{
    await app.Services.EnsureMigratedAndSeededAsync();
}

// Middleware
// Allow enabling Swagger in non-Development via config/env flag
var enableSwagger =
    app.Configuration.GetValue<bool>("EnableSwagger")
    || app.Configuration.GetValue<bool>("ENABLE_SWAGGER");

if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductService v1");
        // Serve Swagger UI at application root to avoid path issues behind proxies
        c.RoutePrefix = string.Empty;
    });
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "ProductService" }));
app.MapControllers();

app.Run();

// Domain & DbContext moved to Domain/ and Data/ folders.
