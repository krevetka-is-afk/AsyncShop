using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;
using OrdersService.Infrastructure.Data;

namespace OrdersService.Application.Services;

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

            if (messages.Any())
            {
                Console.WriteLine($"[Outbox] Found {messages.Count} messages to process");
            }

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
                    Console.WriteLine($"[Outbox] Error processing message {message.Id}: {e.Message}");
                }
            }

            if (messages.Any())
            {
                await db.SaveChangesAsync(stoppingToken);
            }

            await Task.Delay(1000, stoppingToken); // Лучше вынести в конфиг
        }
    }
}