namespace AngryBirds.Payments.Models;

public class RefundRequest
{
    public string TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Reason { get; set; }
}
