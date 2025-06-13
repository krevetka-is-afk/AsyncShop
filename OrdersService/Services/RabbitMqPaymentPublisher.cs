using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata;
using OrdersService.Interfaces;
using OrdersService.Models;
using RabbitMQ.Client;
using IModel = RabbitMQ.Client.IModel;

namespace OrdersService.Services;

public class RabbitMqPaymentPublisher : IPaymentPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    
    private const string QueueName = "orders_created";

    public RabbitMqPaymentPublisher()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "bse2327",
            Password = "hse236",
        };

        _connection = factory.CreateConnection();
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