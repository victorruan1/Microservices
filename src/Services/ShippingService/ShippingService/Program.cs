using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ShippingService.Data;
using ShippingService.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Serilog ---
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ShippingDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

// HTTP client for calling OrderService
builder.Services.AddHttpClient(
    "OrderService",
    (sp, client) =>
    {
        var cfg = sp.GetRequiredService<IConfiguration>();
        var baseUrl = cfg["OrderService:BaseUrl"] ?? "http://localhost:5125";
        client.BaseAddress = new Uri(baseUrl);
    }
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "ShippingService" }));
app.MapControllers();

app.Run();
