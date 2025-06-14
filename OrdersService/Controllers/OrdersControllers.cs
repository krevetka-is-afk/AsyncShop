using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdersService.Data;
using OrdersService.Interfaces;
using OrdersService.Models;
using OrdersService.Services;
using OrdersService.Storage;

namespace OrdersService.Controllers;

[ApiController]
[Route("orders")]
public class OrdersControllers : ControllerBase
{
    // private readonly InMemoryOrderStore _orderStore;
    private readonly IPaymentPublisher _paymentPublisher;
    private readonly OrdersDbContext _dbContext;
    
    public OrdersControllers(OrdersDbContext dbContext, IPaymentPublisher publisher)
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
            AmountOfPayment = request.AmountOfPayment,
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

    [HttpGet("user")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrdersForUser([FromQuery] Guid userId)
    {
        var orders = await _dbContext.Orders
            .Where(o => o.CustomerId == userId)
            .ToListAsync();
        return Ok(orders);
    }
    
    [HttpGet("{orderId}")]
    public async Task<ActionResult<Order>> GetOrderStatus(Guid orderId)
    {
        var order = await _dbContext.Orders.FindAsync(orderId);
        if (order is null) return NotFound("Order not found");
        return Ok(order);
    }

    [HttpPut("{orderId}/update")]
    public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] OrderStatusUpdateRequest request)
    {
        var order = await _dbContext.Orders.FindAsync(orderId);
        if (order == null) return NotFound("Order not found");
        
        order.OrderStatus = request.OrderStatus;
        await _dbContext.SaveChangesAsync();
        return Ok("Order status updated");
    }
}