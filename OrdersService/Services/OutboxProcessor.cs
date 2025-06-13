using Microsoft.EntityFrameworkCore;
using OrdersService.Models;
using OrdersService.Data;
using OrdersService.Interfaces;
using System.Text.Json;

namespace OrdersService.Services;

/// <summary>
/// Фоновый сервис обрабатывающий таблицу OutboxMessages
/// </summary>
public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public OutboxProcessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
            var publisher = scope.ServiceProvider.GetRequiredService<IPaymentPublisher>();
            
            var messages = await db.OutboxMessages
                .Where(m => !m.Processed)
                .OrderBy(m => m.CreatedAt)
                .Take(10)
                .ToListAsync(stoppingToken);

            foreach (var message in messages)
            {
                try
                {
                    if (message.EventType == "OrderCreated")
                    {
                        var order = JsonSerializer.Deserialize<Order>(message.Payload);
                        if (order != null)
                            await publisher.PublishOrderAsync(order);
                        
                        message.Processed = true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Outbox] Error message processed: {e.Message}");
                }
            }
            
            await db.SaveChangesAsync(stoppingToken);
            await Task.Delay(1000, stoppingToken);
        }
    }
}