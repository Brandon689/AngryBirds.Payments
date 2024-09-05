namespace AngryBirds.Payments.Models;

public class PaymentResult
{
    public bool Success { get; set; }
    public string TransactionId { get; set; }
    public string ErrorMessage { get; set; }
    public string Status { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal AmountProcessed { get; set; }
    public string Currency { get; set; }
}