using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;
using OrdersService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace OrdersService.Infrastructure.Repositories;

public class EfOrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _ordersDbContext;

    public EfOrderRepository(OrdersDbContext ordersDbContext)
    {
        _ordersDbContext = ordersDbContext;
    }
    
    public async Task<Order?> GetOrderAsync(Guid orderId)
    {
        return await _ordersDbContext.Orders.FindAsync(orderId);
    }

    public async Task AddOrderAsync(Order order)
    {
        _ordersDbContext.Orders.Add(order);
        await _ordersDbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
       return await _ordersDbContext.Orders.ToListAsync();
    }
}