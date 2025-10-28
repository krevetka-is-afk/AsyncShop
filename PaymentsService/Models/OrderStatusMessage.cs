namespace PaymentsService.Models;

public class OrderStatusMessage
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}