using System.Text;
using System.Text.Json;
using OrdersService.Data;
using OrdersService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrdersService.Services;

public class OrderStatusConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _queueName = "orders_paid";

    public OrderStatusConsumer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "bse2327",
            Password = "hse236"
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare(_queueName, true, false, false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (sender, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var msg = JsonSerializer.Deserialize<OrderStatusMessage>(json);
            if (msg == null) return;

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
            var order = await db.Orders.FindAsync(msg.OrderId);
            if (order == null) return;

            order.OrderStatus = msg.Status == "Paid" ? OrderStatus.Paid : OrderStatus.Failed;
            await db.SaveChangesAsync();

            Console.WriteLine($"[OrdersService] Order {msg.OrderId} to status {msg.Status}");
        };

        channel.BasicConsume(_queueName, autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }
}