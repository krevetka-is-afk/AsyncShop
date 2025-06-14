using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PaymentsService.Models;
using RabbitMQ.Client;

namespace PaymentsService.Services;

public class RabbitMqStatusPublisher
{
    private readonly IModel _channel;
    private readonly string QueueName = "orders_paid";
    private readonly ILogger<RabbitMqStatusPublisher> _logger;

    public RabbitMqStatusPublisher(IConfiguration configuration, ILogger<RabbitMqStatusPublisher> logger)
    {
        _logger = logger;
        
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"],
            UserName = configuration["RabbitMQ:UserName"],
            Password = configuration["RabbitMQ:Password"]
        };
        
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();
        
        _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false);
        _logger.LogInformation("RabbitMqStatusPublisher initialized and connected to queue: {QueueName}", QueueName);
    }

    public void PublishStatus(Guid orderId, string status)
    {
        var message = new OrderStatusMessage
        {
            OrderId = orderId,
            Status = status
        };
        
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        
        _channel.BasicPublish(
            exchange: "",
            routingKey: QueueName,
            basicProperties: null,
            body: body
            );
        _logger.LogInformation("[RabbitMQ] Order {OrderId} status {Status} published", orderId, status);
    }
}