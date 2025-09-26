using System.Text.Json.Serialization;
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
builder.Services.AddDbContext<OrderDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

// Register only the direct-to-queue RabbitMQ publisher
builder.Services.AddSingleton<OrderQueuePublisher>();

var app = builder.Build();

// Apply migrations (will create database if it doesn't exist)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "OrderService" }));

app.MapGet("/orders", async (OrderDbContext db) => await db.Orders.AsNoTracking().ToListAsync());
app.MapPost(
    "/orders",
    async (OrderDbContext db, Order order, OrderQueuePublisher queuePublisher) =>
    {
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var completedAt = DateTime.UtcNow;

        // Publish detailed message to queue
        await queuePublisher.PublishOrderCompletedAsync(order, completedAt);

        return Results.Created($"/orders/{order.Id}", order);
    }
);

// Update order shipping status (called by ShippingService)
app.MapPut(
    "/orders/{id:int}/shipping-status",
    async (OrderDbContext db, int id, ShippingStatusUpdateDto dto) =>
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null)
            return Results.NotFound();
        order.Order_Status = dto.Status;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
);

app.Run();

class Order
{
    public int Id { get; set; }
    public DateTime Order_Date { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int PaymentMethodId { get; set; }
    public string PaymentName { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingMethod { get; set; } = string.Empty;
    public decimal BillAmount { get; set; }
    public string Order_Status { get; set; } = string.Empty;

    // Navigation property → one order has many details
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

class OrderDetail
{
    public int Id { get; set; }
    public int Order_Id { get; set; } // FK
    public int Product_Id { get; set; }
    public string Product_name { get; set; } = string.Empty;
    public int Qty { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }

    // Navigation property back to Order
    [JsonIgnore]
    public Order? Order { get; set; }
}

class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Order>()
            .HasMany(o => o.OrderDetails)
            .WithOne(od => od.Order)
            .HasForeignKey(od => od.Order_Id);
    }
}

// DTO for updating shipping status
public record ShippingStatusUpdateDto(string Status);
