using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PaymentsService.Services;

public class OrderConsumer : BackgroundService
{
    private IConnection? _connection;
    private IModel? _channel;
    
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
        consumer.Received += (sender, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            Console.WriteLine("[PaymentService] Received order {0}", json);
            
            // TODO: deserialize and process bill
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