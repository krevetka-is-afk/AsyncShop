namespace OrdersService.Models;

public enum OrderStatus
{
    Created = 0,
    PaymentPending,
    Paid,
    Failed
}