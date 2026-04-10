using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ShippingService.Api.Data;
using ShippingService.Api.Models;

namespace ShippingService.Api.Services;

public class OrderEventConsumer : BackgroundService
{
    private readonly ILogger<OrderEventConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    public OrderEventConsumer(ILogger<OrderEventConsumer> logger, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(10000, stoppingToken); // Wait 10 seconds for RabbitMQ to be ready

        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMqHost"] ?? "localhost"
        };

        var retryCount = 0;
        var maxRetries = 5;

        while (retryCount < maxRetries && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Create separate queues for shipping service
                _channel.QueueDeclare(queue: "shipping_order_created", durable: true, exclusive: false, autoDelete: false);
                _channel.QueueDeclare(queue: "shipping_order_deleted", durable: true, exclusive: false, autoDelete: false);

                var orderCreatedConsumer = new EventingBasicConsumer(_channel);
                orderCreatedConsumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($"Received OrderCreated event: {message}");

                    try
                    {
                        var orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message);
                        if (orderCreatedEvent != null)
                        {
                            // Use Task.Run to properly execute async work in background
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await CreateShipment(orderCreatedEvent.OrderId);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error creating shipment in background task");
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing OrderCreated event");
                    }
                };

                var orderDeletedConsumer = new EventingBasicConsumer(_channel);
                orderDeletedConsumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($"Received OrderDeleted event: {message}");
                };

                _channel.BasicConsume(queue: "shipping_order_created", autoAck: true, consumer: orderCreatedConsumer);
                _channel.BasicConsume(queue: "shipping_order_deleted", autoAck: true, consumer: orderDeletedConsumer);

                _logger.LogInformation("Shipping service is listening for order events");

                await Task.Delay(Timeout.Infinite, stoppingToken);
                break; // Exit retry loop on success
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogError(ex, $"Failed to connect to RabbitMQ (attempt {retryCount}/{maxRetries})");

                if (retryCount < maxRetries)
                {
                    await Task.Delay(5000, stoppingToken); // Wait 5 seconds before retrying
                }
            }
        }

        if (retryCount >= maxRetries)
        {
            _logger.LogError("Failed to connect to RabbitMQ after maximum retries");
        }
    }

    private async Task CreateShipment(int orderId)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ShippingDbContext>();

        var shipment = new Shipment
        {
            OrderId = orderId,
            TrackingNumber = Guid.NewGuid().ToString("N")[..12].ToUpper(),
            CreatedAt = DateTime.UtcNow
        };

        await context.Shipments.AddAsync(shipment);
        await context.SaveChangesAsync();

        _logger.LogInformation($"Created shipment for order {orderId}");
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}

public class OrderCreatedEvent
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}
