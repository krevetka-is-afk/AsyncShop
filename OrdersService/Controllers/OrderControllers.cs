using Microsoft.AspNetCore.Mvc;
using OrdersService.Data;
using OrdersService.Interfaces;
using OrdersService.Models;
using OrdersService.Services;
using OrdersService.Storage;

namespace OrdersService.Controllers;

[ApiController]
[Route("orders")]
public class OrderControllers : ControllerBase
{
    // private readonly InMemoryOrderStore _orderStore;
    private readonly IPaymentPublisher _paymentPublisher;
    private readonly OrdersDbContext _dbContext;
    
    public OrderControllers(OrdersDbContext dbContext, IPaymentPublisher publisher)
    {
        _dbContext = dbContext;
        _paymentPublisher = publisher;
    }

    [HttpPost("create")]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] OrderRequest request)
    {
        await using var tx = await _dbContext.Database.BeginTransactionAsync();

        var order = new Order
        {
            CustomerId = request.CustomerId,
            Amount = request.Amount,
            OrderStatus = OrderStatus.PaymentPending
        };
        
        _dbContext.Orders.Add(order);

        var outbox = new OutboxMessage
        {
            EventType = "OrderCreated",
            Payload = System.Text.Json.JsonSerializer.Serialize(order)
        };
        
        _dbContext.OutboxMessages.Add(outbox);
        
        await _dbContext.SaveChangesAsync();
        await tx.CommitAsync();
        
        return Ok(order);

    }

    // [HttpGet("user")]
    // public ActionResult<IEnumerable<Order>> GetOrdersForUser([FromQuery] Guid userId)
    // {
    //     var orders = _orderStore.GetOrders(userId);
    //     return Ok(orders);
    // }
    //
    // [HttpGet("{orderId}")]
    // public ActionResult<Order> GetOrderStatus(Guid orderId)
    // {
    //     var order = _orderStore.GetById(orderId);
    //     if (order == null)
    //         return NotFound("Order not found");
    //     return Ok(order);
    // }
}