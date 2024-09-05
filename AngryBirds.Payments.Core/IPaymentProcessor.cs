using AngryBirds.Payments.Models;

namespace AngryBirds.Payments.Core;

public interface IPaymentProcessor
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
    Task<RefundResult> ProcessRefundAsync(RefundRequest request);
    Task<TransactionDetails> GetTransactionDetailsAsync(string transactionId);
    Task<SubscriptionResult> CreateSubscriptionAsync(SubscriptionRequest request);
    Task<SubscriptionResult> CancelSubscriptionAsync(string subscriptionId);

    Task<CustomerResult> CreateCustomerAsync(CustomerRequest request);
    Task<CustomerResult> GetCustomerAsync(string customerId);
    Task<CustomerResult> UpdateCustomerAsync(string customerId, CustomerRequest request);

    Task<PlanResult> CreatePlanAsync(PlanRequest request);

    Task<PaymentMethodResult> AddPaymentMethodToCustomerAsync(string customerId, PaymentMethodInfo paymentMethod);
}