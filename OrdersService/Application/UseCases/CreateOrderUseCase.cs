using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.UseCases;

public class CreateOrderUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentPublisher _paymentPublisher;

    public CreateOrderUseCase(IOrderRepository orderRepository, IPaymentPublisher paymentPublisher)
    {
        _orderRepository = orderRepository;
        _paymentPublisher = paymentPublisher;
    }

    public async Task<Guid> ExecuteTaskAsync(Guid customerId, decimal amount)
    {
        var order = new Order
        {
            CustomerId = customerId,
            AmountOfPayment = amount
        };
        
        await _orderRepository.AddOrderAsync(order);
        await _paymentPublisher.PublishOrderAsync(order);

        return order.OrderId;
    }
}