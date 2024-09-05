namespace AngryBirds.Payments.Models;

public class TransactionDetails
{
    public string TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Status { get; set; }
    public DateTime Timestamp { get; set; }
    public string PaymentMethodType { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string Description { get; set; }
}