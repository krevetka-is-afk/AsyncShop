using System.Text;
using System.Text.Json;
using PaymentsService.Models;
using RabbitMQ.Client;

namespace PaymentsService.Services;

public class RabbitMqStatusPublisher
{
    private readonly IModel _channel;
    private readonly string QueueName = "order_paid";

    public RabbitMqStatusPublisher()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "bse2327",
            Password = "hse236"
        };
        
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();
        
        _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false);
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
        Console.WriteLine("[RabbitMQ] Order {1} status {0} published", status, orderId);
    }
    
}