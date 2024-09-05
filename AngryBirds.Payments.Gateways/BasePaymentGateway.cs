using AngryBirds.Payments.Core;
using AngryBirds.Payments.Models;

namespace AngryBirds.Payments.Gateways;

public abstract class BasePaymentGateway : IPaymentGateway
{
    public abstract Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
    public abstract Task<RefundResult> ProcessRefundAsync(RefundRequest request);
    public abstract Task<TransactionDetails> GetTransactionDetailsAsync(string transactionId);

    public abstract Task<SubscriptionResult> CreateSubscriptionAsync(SubscriptionRequest request);
    public abstract Task<SubscriptionResult> CancelSubscriptionAsync(string subscriptionId);

    public abstract Task<CustomerResult> CreateCustomerAsync(CustomerRequest request);
    public abstract Task<CustomerResult> GetCustomerAsync(string customerId);
    public abstract Task<CustomerResult> UpdateCustomerAsync(string customerId, CustomerRequest request);

    public abstract Task<PlanResult> CreatePlanAsync(PlanRequest request);

    public abstract Task<PaymentMethodResult> AddPaymentMethodToCustomerAsync(string customerId, PaymentMethodInfo paymentMethod);
}