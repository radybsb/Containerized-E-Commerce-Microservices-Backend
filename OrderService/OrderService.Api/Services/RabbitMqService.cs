using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace OrderService.Api.Services;

public class RabbitMqService : IRabbitMqService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqService> _logger;

    public RabbitMqService(IConfiguration configuration, ILogger<RabbitMqService> logger)
    {
        _logger = logger;
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMqHost"] ?? "localhost"
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _logger.LogInformation("Connected to RabbitMQ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ");
            throw;
        }
    }

    public void PublishMessage<T>(string queueName, T message)
    {
        try
        {
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(exchange: string.Empty, routingKey: queueName, basicProperties: null, body: body);

            _logger.LogInformation($"Published message to queue {queueName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to publish message to queue {queueName}");
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
