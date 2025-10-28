namespace OrdersService.Models;

public class Order
{
    public Guid OrderId { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public decimal AmountOfPayment { get; set; }
    public DateTime OrderCreated { get; set; } = DateTime.UtcNow;
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Created;
}