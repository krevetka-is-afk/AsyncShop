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
    private readonly string _queueName = "orders_created";
    private readonly IConfiguration _configuration;

    public OrderConsumer(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"],
            UserName = _configuration["RabbitMQ:UserName"],
            Password = _configuration["RabbitMQ:Password"],
        };
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        
        _channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);
        
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (sender, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            Console.WriteLine("[PaymentService] Received order {0}", json);
            
            try
            {
                var order = JsonSerializer.Deserialize<OrderMessage>(json);
                if (order is null) return;

                using var scope = _serviceProvider.CreateScope();
                var accountService = scope.ServiceProvider.GetRequiredService<AccountService>();
                // var statusClient = scope.ServiceProvider.GetRequiredService<OrderStatusClient>();
                var publisher = scope.ServiceProvider.GetRequiredService<RabbitMqStatusPublisher>();

                var success = await accountService.TryWithdrawAccountAsync(order.CustomerId, order.AmountOfPayment);
                Console.WriteLine(success
                    ? $"[Payment] Успешно списано {order.AmountOfPayment} для {order.CustomerId}"
                    : $"[Payment] Недостаточно средств для {order.CustomerId}");
                
                var status = success ? "Paid" : "Failed"; 
                publisher.PublishStatus(order.OrderId, status);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Payment ERROR] {ex.Message}");
            }
        };
        
        _channel.BasicConsume(_queueName, autoAck: true, consumer: consumer);
        
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}