namespace OrdersService.Models;

public enum OrderStatus
{
    Created,
    PaymentPending,
    Paid,
    Failed
}