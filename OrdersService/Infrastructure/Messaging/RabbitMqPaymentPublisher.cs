using System.Text;
using System.Text.Json;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;
using RabbitMQ.Client;
using IModel = RabbitMQ.Client.IModel;

namespace OrdersService.Infrastructure.Messaging;

public class RabbitMqPaymentPublisher : IPaymentPublisher
{
    private readonly IModel _channel;
    private IConnection? _connection;
    
    private const string QueueName = "orders_created";

    public RabbitMqPaymentPublisher(IConfiguration config)
    {
        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:HostName"],
            UserName = config["RabbitMQ:UserName"],
            Password = config["RabbitMQ:Password"]
        };

        int retries = 0;
        while (true)
        {
            try
            {
                _connection = factory.CreateConnection();
                break;
            }
            catch
            {
                if (++retries > 10) throw;
                Console.WriteLine("Retrying RabbitMQ connection...");
                Thread.Sleep(3000);
            }
        }
        _channel = _connection.CreateModel();
        
        _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);

    }
    
    public Task PublishOrderAsync(Order order)
    {
        var message = JsonSerializer.Serialize(order);
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(
            exchange: "",
            routingKey: QueueName,
            basicProperties: null,
            body: body);

        Console.WriteLine($"[RabbitMQ] Published Order {order.OrderId}");

        return Task.CompletedTask;
    }
    
}