namespace AngryBirds.Payments.Models;
public class PaymentMethodInfo
{
    public string Type { get; set; } // e.g., "CreditCard", "PayPal", "BankTransfer"
    public string Details { get; set; } // JSON string containing method-specific details
}
