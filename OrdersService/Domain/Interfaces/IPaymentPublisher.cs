using OrdersService.Models;

namespace OrdersService.Interfaces;

public interface IPaymentPublisher
{
    Task PublishOrderAsync(Order order);
}