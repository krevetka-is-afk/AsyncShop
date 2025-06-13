using OrdersService.Models;

namespace OrdersService.Storage;

public class InMemoryOrderStore
{
    private readonly List<Order> _orders = new();

    public Order CreateOrder(Guid userId, decimal amount)
    {
        var order = new Order
        {
            CustomerId = userId,
            Amount = amount,
            OrderStatus = OrderStatus.PaymentPending
        };
        _orders.Add(order);
        return order;
    }

    public IEnumerable<Order> GetOrders(Guid userId)
    {
        return _orders.Where(o => o.CustomerId == userId);
    }

    public Order? GetById(Guid orderId)
    {
        return _orders.FirstOrDefault(o => o.OrderId == orderId);
    }

    public void UpdateStatus(Guid orderId, OrderStatus status)
    {
        var order = GetById(orderId);
        if (order != null)
        {
            order.OrderStatus = status;
        }
    }
}