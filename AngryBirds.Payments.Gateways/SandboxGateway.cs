using AngryBirds.Payments.Models;

namespace AngryBirds.Payments.Gateways;

public class SandboxGateway : BasePaymentGateway
{
    public override async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        // Simulate processing delay
        await Task.Delay(500);

        // Sandbox implementation
        var success = request.Amount > 0 && request.Amount < 1000000; // Succeed for reasonable amounts

        return new PaymentResult
        {
            Success = success,
            TransactionId = success ? $"SANDBOX-{Guid.NewGuid()}" : null,
            ErrorMessage = success ? null : "Payment amount out of allowed range",
            Status = success ? "Completed" : "Failed",
            Timestamp = DateTime.UtcNow,
            AmountProcessed = success ? request.Amount : 0,
            Currency = request.Currency
        };
    }

    public override async Task<RefundResult> ProcessRefundAsync(RefundRequest request)
    {
        // Simulate processing delay
        await Task.Delay(500);

        // Sandbox implementation
        var success = request.Amount > 0 && !string.IsNullOrEmpty(request.TransactionId);

        return new RefundResult
        {
            Success = success,
            RefundId = success ? $"SANDBOX-REFUND-{Guid.NewGuid()}" : null,
            ErrorMessage = success ? null : "Invalid refund request",
            RefundedAmount = success ? request.Amount : 0
        };
    }

    public override async Task<TransactionDetails> GetTransactionDetailsAsync(string transactionId)
    {
        // Simulate processing delay
        await Task.Delay(500);

        // For sandbox, we'll create a fake transaction detail
        return new TransactionDetails
        {
            TransactionId = transactionId,
            Amount = 100.00m, // Example amount
            Currency = "USD",
            Status = "succeeded",
            Timestamp = DateTime.UtcNow.AddMinutes(-5), // Assume transaction happened 5 minutes ago
            PaymentMethodType = "card",
            CustomerName = "John Doe",
            CustomerEmail = "john.doe@example.com",
            Description = "Sandbox transaction"
        };
    }

    public override async Task<SubscriptionResult> CreateSubscriptionAsync(SubscriptionRequest request)
    {
        // Simulate processing delay
        await Task.Delay(500);

        // Sandbox implementation
        var success = request.Amount > 0 && !string.IsNullOrEmpty(request.CustomerId);

        return new SubscriptionResult
        {
            Success = success,
            SubscriptionId = success ? $"SANDBOX-SUB-{Guid.NewGuid()}" : null,
            Status = success ? "active" : "failed",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(request.IntervalCount),
            ErrorMessage = success ? null : "Invalid subscription request"
        };
    }

    public override async Task<SubscriptionResult> CancelSubscriptionAsync(string subscriptionId)
    {
        // Simulate processing delay
        await Task.Delay(500);

        // Sandbox implementation
        var success = !string.IsNullOrEmpty(subscriptionId);

        return new SubscriptionResult
        {
            Success = success,
            SubscriptionId = subscriptionId,
            Status = success ? "cancelled" : "failed",
            StartDate = DateTime.UtcNow.AddMonths(-1), // Assume subscription started a month ago
            EndDate = DateTime.UtcNow,
            ErrorMessage = success ? null : "Invalid subscription ID"
        };
    }
    public override Task<PaymentMethodResult> AddPaymentMethodToCustomerAsync(string customerId, PaymentMethodInfo paymentMethod)
    {
        // Sandbox implementation
        return Task.FromResult(new PaymentMethodResult
        {
            Success = true,
            PaymentMethodId = $"pm_sandbox_{Guid.NewGuid()}",
            Type = paymentMethod.Type
        });
    }

    public override Task<CustomerResult> CreateCustomerAsync(CustomerRequest request)
    {
        throw new NotImplementedException("CreateCustomerAsync is not implemented for PayPalGateway");
    }

    public override Task<CustomerResult> GetCustomerAsync(string customerId)
    {
        throw new NotImplementedException("GetCustomerAsync is not implemented for PayPalGateway");
    }

    public override Task<CustomerResult> UpdateCustomerAsync(string customerId, CustomerRequest request)
    {
        throw new NotImplementedException("UpdateCustomerAsync is not implemented for PayPalGateway");
    }

    public override Task<PlanResult> CreatePlanAsync(PlanRequest request)
    {
        throw new NotImplementedException();
    }
}