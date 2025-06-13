using OrdersService.Models;
using OrdersService.Storage;

namespace OrdersService.Services;

public class FakePaymentProcessor
{
    private readonly InMemoryOrderStore _orderStore;

    public FakePaymentProcessor(InMemoryOrderStore orderStore)
    {
        _orderStore = orderStore;
    }

    public async Task ProcessPaymentAsync(Order order)
    {
        await Task.Delay(1000);

        var success = order.Amount <= 500;
        
        var newStatus = success ? OrderStatus.Paid : OrderStatus.Failed;
        _orderStore.UpdateStatus(order.OrderId, newStatus);
        
        Console.WriteLine($"[Payment] Order {order.OrderId} status -> {newStatus}");
    }
}