namespace AngryBirds.Payments.Models;

public class CustomerRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Description { get; set; }
}

public class PaymentMethodResult
{
    public bool Success { get; set; }
    public string PaymentMethodId { get; set; }
    public string Type { get; set; }
    public string ErrorMessage { get; set; }
}