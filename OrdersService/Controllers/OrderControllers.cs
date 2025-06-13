using Microsoft.AspNetCore.Mvc;
using OrdersService.Models;
using OrdersService.Services;
using OrdersService.Storage;

namespace OrdersService.Controllers;

[ApiController]
[Route("orders")]
public class OrderControllers : ControllerBase
{
    private readonly InMemoryOrderStore _orderStore;
    private readonly FakePaymentProcessor _paymentProcessor;

    public OrderControllers(InMemoryOrderStore orderStore, FakePaymentProcessor fakePaymentProcessor)
    {
        _orderStore = orderStore;
        _paymentProcessor = fakePaymentProcessor;
    }

    [HttpPost("create")]
    public IActionResult CreateOrder([FromBody] OrderRequest request)
    {
        var order = _orderStore.CreateOrder(request.CustomerId, request.Amount);
        
        _ = Task.Run(() => _paymentProcessor.ProcessPaymentAsync(order));
        
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