using Microsoft.AspNetCore.Mvc;
using OrdersService.Interfaces;
using OrdersService.Models;
using OrdersService.Services;
using OrdersService.Storage;

namespace OrdersService.Controllers;

[ApiController]
[Route("orders")]
public class OrderControllers : ControllerBase
{
    private readonly InMemoryOrderStore _orderStore;
    private readonly IPaymentPublisher _paymentPublisher;

    public OrderControllers(InMemoryOrderStore orderStore, IPaymentPublisher fakePaymentPublisher)
    {
        _orderStore = orderStore;
        _paymentPublisher = fakePaymentPublisher;
    }

    [HttpPost("create")]
    public IActionResult CreateOrder([FromBody] OrderRequest request)
    {
        var order = _orderStore.CreateOrder(request.CustomerId, request.Amount);
        
        _ = Task.Run(() => _paymentPublisher.PublishOrderAsync(order));
        
        return Ok(order);
    }

    [HttpGet("user")]
    public ActionResult<IEnumerable<Order>> GetOrdersForUser([FromQuery] Guid userId)
    {
        var orders = _orderStore.GetOrders(userId);
        return Ok(orders);
    }

    [HttpGet("{orderId}")]
    public ActionResult<Order> GetOrderStatus(Guid orderId)
    {
        var order = _orderStore.GetById(orderId);
        if (order == null)
            return NotFound("Order not found");
        return Ok(order);
    }
}