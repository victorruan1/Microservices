using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PromotionsService.Data;
using PromotionsService.Messaging;

var builder = WebApplication.CreateBuilder(args);

// EF etc...
builder.Services.AddDbContext<PromotionDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("CustomerOrAdmin", p => p.RequireRole("Customer", "Admin"));
});

// ---- Service Bus wiring ----
builder.Services.AddSingleton<ServiceBusClient>(sp =>
{
    var cs = sp.GetRequiredService<IConfiguration>()["ServiceBus:ConnectionString"];
    if (string.IsNullOrWhiteSpace(cs))
        throw new InvalidOperationException("ServiceBus:ConnectionString is missing.");

    return new ServiceBusClient(cs);
});

builder.Services.AddSingleton<IPromotionEventPublisher, AzureServiceBusPromotionPublisher>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
