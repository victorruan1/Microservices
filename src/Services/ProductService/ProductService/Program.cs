using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("CustomerOrAdmin", p => p.RequireRole("Customer", "Admin"));
});
builder.Services.AddControllers();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Dev", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

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

app.UseCors("Dev");
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "ProductService" }));
app.MapControllers();

app.Run();

// Domain & DbContext moved to Domain/ and Data/ folders.
