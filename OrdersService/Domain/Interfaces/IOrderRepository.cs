using OrdersService.Domain.Entities;

namespace OrdersService.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetOrderAsync(Guid orderId);
    Task AddOrderAsync(Order order);
    Task<IEnumerable<Order>> GetAllOrdersAsync();
}