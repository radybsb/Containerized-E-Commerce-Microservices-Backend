namespace CustomerService.Api.Services;

public interface IRabbitMqService
{
    void PublishMessage<T>(string queueName, T message);
}
