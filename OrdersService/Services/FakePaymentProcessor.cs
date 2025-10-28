using OrdersService.Models;
using OrdersService.Storage;
using OrdersService.Interfaces;

namespace OrdersService.Services;

public class FakePaymentProcessor : IPaymentPublisher
{
    private readonly InMemoryOrderStore _orderStore;

    public FakePaymentProcessor(InMemoryOrderStore orderStore)
    {
        _orderStore = orderStore;
    }

    public async Task PublishOrderAsync(Order order)
    {
        await Task.Delay(1000);

        var success = order.AmountOfPayment <= 500;
        
        var newStatus = success ? OrderStatus.Paid : OrderStatus.Failed;
        _orderStore.UpdateStatus(order.OrderId, newStatus);
        
        Console.WriteLine($"[Payment] Order {order.OrderId} status -> {newStatus}");
    }
}