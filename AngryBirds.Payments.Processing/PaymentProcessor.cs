using AngryBirds.Payments.Core;
using AngryBirds.Payments.Exceptions;
using AngryBirds.Payments.Models;
using Serilog;
using System.ComponentModel.DataAnnotations;

namespace AngryBirds.Payments.Processing;

public class PaymentProcessor : IPaymentProcessor
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger _logger;

    public PaymentProcessor(IPaymentGateway paymentGateway)
    {
        _paymentGateway = paymentGateway;
        _logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/payments.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        {
            var errors = validationResults.Select(r => r.ErrorMessage);
            _logger.Warning("Payment request validation failed: {Errors}", string.Join(", ", errors));
            throw new PaymentException("Invalid payment request", "INVALID_REQUEST");
        }
        try
        {
            _logger.Information("Processing payment for {Amount} {Currency}", request.Amount, request.Currency);
            var result = await _paymentGateway.ProcessPaymentAsync(request);
            if (result.Success)
            {
                _logger.Information("Payment processed successfully. Transaction ID: {TransactionId}", result.TransactionId);
            }
            else
            {
                _logger.Warning("Payment processing failed. Error: {ErrorMessage}", result.ErrorMessage);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while processing the payment");
            throw new PaymentException("An unexpected error occurred while processing the payment", "UNEXPECTED_ERROR", ex);
        }
    }

    public async Task<RefundResult> ProcessRefundAsync(RefundRequest request)
    {
        try
        {
            _logger.Information("Processing refund for {Amount} {Currency}", request.Amount, request.Currency);
            var result = await _paymentGateway.ProcessRefundAsync(request);
            if (result.Success)
            {
                _logger.Information("Refund processed successfully. Refund ID: {RefundId}", result.RefundId);
            }
            else
            {
                _logger.Warning("Refund processing failed. Error: {ErrorMessage}", result.ErrorMessage);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while processing the refund");
            throw new PaymentException("An unexpected error occurred while processing the refund", "UNEXPECTED_ERROR", ex);
        }
    }

    public async Task<TransactionDetails> GetTransactionDetailsAsync(string transactionId)
    {
        try
        {
            _logger.Information("Retrieving transaction details for Transaction ID: {TransactionId}", transactionId);
            var result = await _paymentGateway.GetTransactionDetailsAsync(transactionId);
            _logger.Information("Successfully retrieved transaction details for Transaction ID: {TransactionId}", transactionId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while retrieving transaction details for Transaction ID: {TransactionId}", transactionId);
            throw new PaymentException("An unexpected error occurred while retrieving transaction details", "UNEXPECTED_ERROR", ex);
        }
    }

    public async Task<SubscriptionResult> CreateSubscriptionAsync(SubscriptionRequest request)
    {
        try
        {
            _logger.Information("Creating subscription for Customer ID: {CustomerId}", request.CustomerId);
            var result = await _paymentGateway.CreateSubscriptionAsync(request);
            if (result.Success)
            {
                _logger.Information("Successfully created subscription. Subscription ID: {SubscriptionId}", result.SubscriptionId);
            }
            else
            {
                _logger.Warning("Failed to create subscription. Error: {ErrorMessage}", result.ErrorMessage);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while creating the subscription");
            throw new PaymentException("An unexpected error occurred while creating the subscription", "UNEXPECTED_ERROR", ex);
        }
    }

    public async Task<SubscriptionResult> CancelSubscriptionAsync(string subscriptionId)
    {
        try
        {
            _logger.Information("Cancelling subscription. Subscription ID: {SubscriptionId}", subscriptionId);
            var result = await _paymentGateway.CancelSubscriptionAsync(subscriptionId);
            if (result.Success)
            {
                _logger.Information("Successfully cancelled subscription. Subscription ID: {SubscriptionId}", subscriptionId);
            }
            else
            {
                _logger.Warning("Failed to cancel subscription. Error: {ErrorMessage}", result.ErrorMessage);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while cancelling the subscription");
            throw new PaymentException("An unexpected error occurred while cancelling the subscription", "UNEXPECTED_ERROR", ex);
        }
    }







    public async Task<CustomerResult> CreateCustomerAsync(CustomerRequest request)
    {
        try
        {
            _logger.Information("Creating customer: {CustomerName}", request.Name);
            var result = await _paymentGateway.CreateCustomerAsync(request);
            if (result.Success)
            {
                _logger.Information("Successfully created customer. Customer ID: {CustomerId}", result.CustomerId);
            }
            else
            {
                _logger.Warning("Failed to create customer. Error: {ErrorMessage}", result.ErrorMessage);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while creating the customer");
            throw new PaymentException("An unexpected error occurred while creating the customer", "UNEXPECTED_ERROR", ex);
        }
    }

    public async Task<CustomerResult> GetCustomerAsync(string customerId)
    {
        try
        {
            _logger.Information("Retrieving customer. Customer ID: {CustomerId}", customerId);
            var result = await _paymentGateway.GetCustomerAsync(customerId);
            if (result.Success)
            {
                _logger.Information("Successfully retrieved customer. Customer ID: {CustomerId}", customerId);
            }
            else
            {
                _logger.Warning("Failed to retrieve customer. Error: {ErrorMessage}", result.ErrorMessage);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while retrieving the customer");
            throw new PaymentException("An unexpected error occurred while retrieving the customer", "UNEXPECTED_ERROR", ex);
        }
    }

    public async Task<CustomerResult> UpdateCustomerAsync(string customerId, CustomerRequest request)
    {
        try
        {
            _logger.Information("Updating customer. Customer ID: {CustomerId}", customerId);
            var result = await _paymentGateway.UpdateCustomerAsync(customerId, request);
            if (result.Success)
            {
                _logger.Information("Successfully updated customer. Customer ID: {CustomerId}", customerId);
            }
            else
            {
                _logger.Warning("Failed to update customer. Error: {ErrorMessage}", result.ErrorMessage);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while updating the customer");
            throw new PaymentException("An unexpected error occurred while updating the customer", "UNEXPECTED_ERROR", ex);
        }
    }

    public async Task<PlanResult> CreatePlanAsync(PlanRequest request)
    {
        try
        {
            _logger.Information("Creating plan: {PlanName}", request.Name);
            var result = await _paymentGateway.CreatePlanAsync(request);
            if (result.Success)
            {
                _logger.Information("Successfully created plan. Plan ID: {PlanId}", result.PlanId);
            }
            else
            {
                _logger.Warning("Failed to create plan. Error: {ErrorMessage}", result.ErrorMessage);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while creating the plan");
            throw new PaymentException("An unexpected error occurred while creating the plan", "UNEXPECTED_ERROR", ex);
        }
    }


    public async Task<PaymentMethodResult> AddPaymentMethodToCustomerAsync(string customerId, PaymentMethodInfo paymentMethod)
    {
        try
        {
            _logger.Information("Adding payment method to customer. Customer ID: {CustomerId}", customerId);
            var result = await _paymentGateway.AddPaymentMethodToCustomerAsync(customerId, paymentMethod);
            if (result.Success)
            {
                _logger.Information("Successfully added payment method. Payment Method ID: {PaymentMethodId}", result.PaymentMethodId);
            }
            else
            {
                _logger.Warning("Failed to add payment method. Error: {ErrorMessage}", result.ErrorMessage);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while adding the payment method to the customer");
            throw new PaymentException("An unexpected error occurred while adding the payment method to the customer", "UNEXPECTED_ERROR", ex);
        }
    }
}
