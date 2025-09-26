using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace EShop.Messaging;

public interface IIntegrationEvent
{
    string EventType => GetType().Name;
    DateTime OccurredUtc { get; }
}

public abstract record IntegrationEventBase(DateTime OccurredUtc) : IIntegrationEvent;

public interface IMessagePublisher
{
    Task PublishAsync<TEvent>(TEvent evt, CancellationToken ct = default)
        where TEvent : IIntegrationEvent;
}

public sealed class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchange;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(RabbitMqSettings settings, ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
            DispatchConsumersAsync = true,
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _exchange = settings.Exchange ?? "eshop.events";
        _channel.ExchangeDeclare(_exchange, ExchangeType.Topic, durable: true, autoDelete: false);
    }

    public Task PublishAsync<TEvent>(TEvent evt, CancellationToken ct = default)
        where TEvent : IIntegrationEvent
    {
        var json = JsonSerializer.Serialize(evt, evt.GetType());
        var body = Encoding.UTF8.GetBytes(json);
        var routingKey = evt.EventType; // can map
        var props = _channel.CreateBasicProperties();
        props.ContentType = "application/json";
        props.DeliveryMode = 2;
        _channel.BasicPublish(_exchange, routingKey, props, body);
        _logger.LogInformation("Published RabbitMQ event {EventType}", evt.EventType);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}

public sealed class ServiceBusPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusPublisher> _logger;

    public ServiceBusPublisher(ServiceBusSettings settings, ILogger<ServiceBusPublisher> logger)
    {
        _logger = logger;
        _client = new ServiceBusClient(settings.ConnectionString);
        _sender = _client.CreateSender(settings.TopicName ?? "eshop-events");
    }

    public async Task PublishAsync<TEvent>(TEvent evt, CancellationToken ct = default)
        where TEvent : IIntegrationEvent
    {
        var json = JsonSerializer.Serialize(evt, evt.GetType());
        var msg = new ServiceBusMessage(json)
        {
            Subject = evt.EventType,
            ContentType = "application/json",
        };
        await _sender.SendMessageAsync(msg, ct);
        _logger.LogInformation("Published ServiceBus event {EventType}", evt.EventType);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}

public record RabbitMqSettings(
    string HostName,
    int Port,
    string UserName,
    string Password,
    string? Exchange
);

public record ServiceBusSettings(string ConnectionString, string? TopicName);

// Event contracts
public record OrderCompletedEvent(
    int OrderId,
    string CustomerName,
    decimal BillAmount,
    string OrderStatus,
    DateTime CompletedUtc
) : IntegrationEventBase(CompletedUtc);

public record PromotionStartedEvent(
    int PromotionId,
    string Code,
    decimal DiscountPercent,
    DateTime StartedUtc
) : IntegrationEventBase(StartedUtc);

public record PromotionEndedEvent(int PromotionId, string Code, DateTime EndedUtc)
    : IntegrationEventBase(EndedUtc);
