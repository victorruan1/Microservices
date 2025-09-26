namespace PromotionsService.Messaging;

using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using PromotionsService.Models;

// Messaging/IPromotionEventPublisher.cs
public interface IPromotionEventPublisher
{
    Task PublishPromotionStartedAsync(Promotion promo);
    Task PublishPromotionEndedAsync(Promotion promo);
}

// Messaging/AzureServiceBusPromotionPublisher.cs
public class AzureServiceBusPromotionPublisher : IPromotionEventPublisher
{
    private readonly ServiceBusSender _sender;

    public AzureServiceBusPromotionPublisher(ServiceBusClient client, IConfiguration cfg)
    {
        // One queue (e.g., "promotion-events")
        var queue = cfg["ServiceBus:PromotionQueue"] ?? "promotion-events";
        _sender = client.CreateSender(queue);
    }

    public Task PublishPromotionStartedAsync(Promotion promo) =>
        SendAsync("PromotionStarted", promo);

    public Task PublishPromotionEndedAsync(Promotion promo) => SendAsync("PromotionEnded", promo);

    private Task SendAsync(string type, Promotion promo)
    {
        var payload = new
        {
            Type = type,
            PromotionId = promo.Id,
            promo.Name,
            Discount = promo.Discount,
            StartDate = promo.StartDate,
            EndDate = promo.EndDate,
        };

        var json = JsonSerializer.Serialize(payload);
        return _sender.SendMessageAsync(new ServiceBusMessage(json) { Subject = type });
    }
}

// No-op publisher fallback
public class NoOpPublisher : IPromotionEventPublisher
{
    public Task PublishPromotionEndedAsync(Promotion promo) => Task.CompletedTask;
    public Task PublishPromotionStartedAsync(Promotion promo) => Task.CompletedTask;
}
