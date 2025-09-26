using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

internal class OrderQueuePublisher : IDisposable
{
    private readonly ILogger<OrderQueuePublisher> _logger;
    private readonly IConnection _connection;
    private readonly string _queueName;

    public OrderQueuePublisher(IConfiguration cfg, ILogger<OrderQueuePublisher> logger)
    {
        _logger = logger;
        var section = cfg.GetSection("RabbitMq");
        var host = section.GetValue<string>("HostName", "localhost");
        var port = section.GetValue<int>("Port", 5672);
        var user = section.GetValue<string>("UserName", "guest");
        var pass = section.GetValue<string>("Password", "guest");
        _queueName = section.GetValue<string>("OrderEventsQueue", "order-events");

        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = user,
            Password = pass,
        };

        _connection = factory.CreateConnection();

        // Ensure queue exists (idempotent)
        using var channel = _connection.CreateModel();
        channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
    }

    internal Task PublishOrderCompletedAsync(Order order, DateTime completedUtc)
    {
        using var channel = _connection.CreateModel();
        // Idempotent declare
        channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var message = OrderCompletedMessage.From(order, completedUtc);
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var props = channel.CreateBasicProperties();
        props.Persistent = true;

        channel.BasicPublish(
            exchange: "",
            routingKey: _queueName,
            basicProperties: props,
            body: body
        );
        _logger.LogInformation(
            "Published OrderCompleted to queue {Queue} for OrderId={OrderId}",
            _queueName,
            order.Id
        );
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}

internal record OrderCompletedItem(
    int Product_Id,
    string Product_name,
    int Qty,
    decimal Price,
    decimal Discount
);

internal record OrderCompletedMessage(
    int OrderId,
    DateTime Order_Date,
    int CustomerId,
    string CustomerName,
    decimal BillAmount,
    string Order_Status,
    string ShippingAddress,
    string ShippingMethod,
    int PaymentMethodId,
    string PaymentName,
    DateTime CompletedUtc,
    IReadOnlyList<OrderCompletedItem> Items
)
{
    internal static OrderCompletedMessage From(Order order, DateTime completedUtc)
    {
        var items = order
            .OrderDetails.Select(d => new OrderCompletedItem(
                d.Product_Id,
                d.Product_name,
                d.Qty,
                d.Price,
                d.Discount
            ))
            .ToList();
        return new OrderCompletedMessage(
            order.Id,
            order.Order_Date,
            order.CustomerId,
            order.CustomerName,
            order.BillAmount,
            order.Order_Status,
            order.ShippingAddress,
            order.ShippingMethod,
            order.PaymentMethodId,
            order.PaymentName,
            completedUtc,
            items
        );
    }
}
