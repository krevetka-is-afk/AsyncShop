namespace PaymentsService.Models;

public class OrderMessage
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal AmountOfPayment { get; set; }
}