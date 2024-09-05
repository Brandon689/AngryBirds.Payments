using System.ComponentModel.DataAnnotations;

namespace AngryBirds.Payments.Models;

public class PaymentRequest : IValidatableObject
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter code")]
    public string Currency { get; set; }

    [Required]
    public PaymentMethodInfo PaymentMethod { get; set; }

    [Required]
    public Customer Customer { get; set; }

    public Address BillingAddress { get; set; }

    [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
    public string Description { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PaymentMethod.Type == "CreditCard" && string.IsNullOrEmpty(PaymentMethod.Details))
        {
            yield return new ValidationResult("Credit card details are required for credit card payments", new[] { nameof(PaymentMethod) });
        }

        if (Amount > 10000 && BillingAddress == null)
        {
            yield return new ValidationResult("Billing address is required for amounts over 10,000", new[] { nameof(BillingAddress) });
        }
    }
}
