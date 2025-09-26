using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
