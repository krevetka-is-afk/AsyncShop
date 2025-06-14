namespace PaymentsService.Models;

/// <summary>
/// запрос оплаты
/// </summary>
public class PaymentRequest
{
    public Guid AccountId { get; set; }
    public decimal AmountOfPayment { get; set; }
}