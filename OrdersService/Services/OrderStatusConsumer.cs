using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using OrdersService.Data;
using OrdersService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrdersService.Services;

public class OrderStatusConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderStatusConsumer> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _queueName = "orders_paid";
    private IConnection? _connection;
    private IModel? _channel;

    public OrderStatusConsumer(
        IServiceProvider serviceProvider,
        ILogger<OrderStatusConsumer> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await InitializeRabbitMq(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    private async Task InitializeRabbitMq(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"],
            UserName = _configuration["RabbitMQ:UserName"],
            Password = _configuration["RabbitMQ:Password"],
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(_queueName, true, false, false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (sender, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogInformation("Received message: {Message}", json);

                var msg = JsonSerializer.Deserialize<OrderStatusMessage>(json);
                if (msg == null)
                {
                    _logger.LogWarning("Failed to deserialize message: {Message}", json);
                    return;
                }

                _logger.LogInformation("Processing order {OrderId} with status {Status}", msg.OrderId, msg.Status);

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
                var order = await db.Orders.FindAsync(msg.OrderId);
                
                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found in database", msg.OrderId);
                    return;
                }

                order.OrderStatus = msg.Status == "Paid" ? OrderStatus.Paid : OrderStatus.Failed;
                await db.SaveChangesAsync();

                _logger.LogInformation("Successfully updated order {OrderId} to status {Status}", msg.OrderId, msg.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {Message}", 
                    ea.Body.ToArray().Length > 0 ? Encoding.UTF8.GetString(ea.Body.ToArray()) : "Empty message");
            }
        };

        _channel.BasicConsume(_queueName, autoAck: true, consumer: consumer);

        _logger.LogInformation("OrderStatusConsumer started and listening on queue: {QueueName}", _queueName);

        // Keep the service running until cancellation is requested
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping OrderStatusConsumer...");
        
        if (_channel != null && _channel.IsOpen)
        {
            _channel.Close();
        }
        
        if (_connection != null && _connection.IsOpen)
        {
            _connection.Close();
        }

        await base.StopAsync(cancellationToken);
    }
}