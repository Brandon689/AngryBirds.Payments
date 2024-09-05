namespace AngryBirds.Payments.Models;

public class Transaction
{
    public string Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public DateTime Timestamp { get; set; }
    public string Status { get; set; }
    public PaymentMethodInfo PaymentMethod { get; set; }
    public Customer Customer { get; set; }
    public Address BillingAddress { get; set; }
}