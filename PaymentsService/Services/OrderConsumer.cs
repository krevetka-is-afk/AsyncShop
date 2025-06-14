using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using PaymentsService.Models;

namespace PaymentsService.Services;

public class OrderConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IModel? _channel;

    public OrderConsumer(IServiceProvider serviceProvider)
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
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.QueueDeclare("orders_created", durable: true, exclusive: false, autoDelete: false);
        
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (sender, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            Console.WriteLine("[PaymentService] Received order {0}", json);
            
            // TODO: deserialize and process bill
            try
            {
                var order = JsonSerializer.Deserialize<OrderMessage>(json);
                if (order is null) return;

                using var scope = _serviceProvider.CreateScope();
                var accountService = scope.ServiceProvider.GetRequiredService<AccountService>();

                var success = await accountService.TryWithdrawAccountAsync(order.CustomerId, order.AmountOfPayment);
                Console.WriteLine(success
                    ? $"[Payment] Успешно списано {order.AmountOfPayment} для {order.CustomerId}"
                    : $"[Payment] Недостаточно средств для {order.CustomerId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Payment ERROR] {ex.Message}");
            }
        };
        
        _channel.BasicConsume("orders_created", autoAck: true, consumer: consumer);
        
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}