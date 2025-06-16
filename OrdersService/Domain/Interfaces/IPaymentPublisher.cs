using OrdersService.Domain.Entities;

namespace OrdersService.Domain.Interfaces;

public interface IPaymentPublisher
{
    Task PublishOrderAsync(Order order);
}