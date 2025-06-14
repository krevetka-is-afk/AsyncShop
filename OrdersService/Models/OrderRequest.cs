namespace OrdersService.Models;

public class OrderRequest
{
    public Guid CustomerId { get; set; }
    public decimal AmountOfPayment { get; set; }
}