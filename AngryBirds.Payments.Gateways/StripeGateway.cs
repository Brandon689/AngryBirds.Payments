using AngryBirds.Payments.Exceptions;
using AngryBirds.Payments.Models;
using Stripe;

namespace AngryBirds.Payments.Gateways;

public class StripeGateway : BasePaymentGateway
{
    private readonly string _apiKey;

    public StripeGateway(string apiKey)
    {
        _apiKey = apiKey;
        StripeConfiguration.ApiKey = _apiKey;
    }

    public override async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100),
                Currency = request.Currency.ToLower(),
                PaymentMethod = request.PaymentMethod.Details,
                Confirm = true,
                Description = request.Description,
                ReceiptEmail = request.Customer.Email,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                    AllowRedirects = "never"
                },
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return new PaymentResult
            {
                Success = paymentIntent.Status == "succeeded",
                TransactionId = paymentIntent.Id,
                Status = paymentIntent.Status,
                AmountProcessed = paymentIntent.Amount / 100m,
                Currency = paymentIntent.Currency,
                Timestamp = DateTime.UtcNow,
                ErrorMessage = paymentIntent.LastPaymentError?.Message
            };
        }
        catch (StripeException e)
        {
            throw new PaymentException($"Stripe error: {e.Message}", e.StripeError.Code, e);
        }
        catch (Exception e)
        {
            throw new PaymentException($"Unexpected error: {e.Message}", "UNEXPECTED_ERROR", e);
        }
    }

    public override async Task<RefundResult> ProcessRefundAsync(RefundRequest request)
    {
        try
        {
            var options = new Stripe.RefundCreateOptions
            {
                PaymentIntent = request.TransactionId,
                Amount = (long)(request.Amount * 100),
                Reason = GetRefundReason(request.Reason)
            };

            var service = new Stripe.RefundService();
            var refund = await service.CreateAsync(options);

            return new RefundResult
            {
                Success = refund.Status == "succeeded",
                RefundId = refund.Id,
                RefundedAmount = refund.Amount / 100m,
                ErrorMessage = refund.FailureReason
            };
        }
        catch (Stripe.StripeException e)
        {
            throw new PaymentException($"Stripe error: {e.Message}", e.StripeError.Code, e);
        }
        catch (Exception e)
        {
            throw new PaymentException($"Unexpected error: {e.Message}", "UNEXPECTED_ERROR", e);
        }
    }

    private string GetRefundReason(string reason)
    {
        return reason.ToLower() switch
        {
            "requested_by_customer" => "requested_by_customer",
            "duplicate" => "duplicate",
            "fraudulent" => "fraudulent",
            _ => "requested_by_customer",
        };
    }

    public override async Task<TransactionDetails> GetTransactionDetailsAsync(string transactionId)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(transactionId);

            return new TransactionDetails
            {
                TransactionId = paymentIntent.Id,
                Amount = paymentIntent.Amount / 100m, // Stripe stores amounts in cents
                Currency = paymentIntent.Currency,
                Status = paymentIntent.Status,
                Timestamp = paymentIntent.Created,
                PaymentMethodType = paymentIntent.PaymentMethodTypes.FirstOrDefault(),
                CustomerName = paymentIntent.Customer?.Name,
                CustomerEmail = paymentIntent.Customer?.Email,
                Description = paymentIntent.Description
            };
        }
        catch (StripeException e)
        {
            throw new PaymentException($"Stripe error: {e.Message}", e.StripeError.Code, e);
        }
        catch (Exception e)
        {
            throw new PaymentException($"Unexpected error: {e.Message}", "UNEXPECTED_ERROR", e);
        }
    }



    public override async Task<SubscriptionResult> CreateSubscriptionAsync(SubscriptionRequest request)
    {
        try
        {
            var options = new SubscriptionCreateOptions
            {
                Customer = request.CustomerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = request.PlanId,
                    },
                },
            };

            var service = new SubscriptionService();
            var subscription = await service.CreateAsync(options);

            return new SubscriptionResult
            {
                Success = true,
                SubscriptionId = subscription.Id,
                Status = subscription.Status,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndedAt
            };
        }
        catch (StripeException e)
        {
            return new SubscriptionResult
            {
                Success = false,
                ErrorMessage = e.Message
            };
        }
    }

    public override async Task<SubscriptionResult> CancelSubscriptionAsync(string subscriptionId)
    {
        try
        {
            var service = new SubscriptionService();
            var subscription = await service.CancelAsync(subscriptionId);

            return new SubscriptionResult
            {
                Success = true,
                SubscriptionId = subscription.Id,
                Status = subscription.Status,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndedAt
            };
        }
        catch (StripeException e)
        {
            return new SubscriptionResult
            {
                Success = false,
                ErrorMessage = e.Message
            };
        }
    }



    public override async Task<CustomerResult> CreateCustomerAsync(CustomerRequest request)
    {
        try
        {
            var options = new CustomerCreateOptions
            {
                Name = request.Name,
                Email = request.Email,
                Description = request.Description
            };

            var service = new CustomerService();
            var customer = await service.CreateAsync(options);

            return new CustomerResult
            {
                Success = true,
                CustomerId = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Description = customer.Description
            };
        }
        catch (StripeException e)
        {
            return new CustomerResult
            {
                Success = false,
                ErrorMessage = e.Message
            };
        }
    }

    public override async Task<CustomerResult> GetCustomerAsync(string customerId)
    {
        try
        {
            var service = new CustomerService();
            var customer = await service.GetAsync(customerId);

            return new CustomerResult
            {
                Success = true,
                CustomerId = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Description = customer.Description
            };
        }
        catch (StripeException e)
        {
            return new CustomerResult
            {
                Success = false,
                ErrorMessage = e.Message
            };
        }
    }

    public override async Task<CustomerResult> UpdateCustomerAsync(string customerId, CustomerRequest request)
    {
        try
        {
            var options = new CustomerUpdateOptions
            {
                Name = request.Name,
                Email = request.Email,
                Description = request.Description
            };

            var service = new CustomerService();
            var customer = await service.UpdateAsync(customerId, options);

            return new CustomerResult
            {
                Success = true,
                CustomerId = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Description = customer.Description
            };
        }
        catch (StripeException e)
        {
            return new CustomerResult
            {
                Success = false,
                ErrorMessage = e.Message
            };
        }
    }

    public override async Task<PlanResult> CreatePlanAsync(PlanRequest request)
    {
        try
        {
            // Create a Product
            var productOptions = new ProductCreateOptions
            {
                Name = request.Name,
                Description = request.Description,
            };
            var productService = new ProductService();
            var product = await productService.CreateAsync(productOptions);

            // Create a Price (which is equivalent to a "Plan" in Stripe's current API)
            var priceOptions = new PriceCreateOptions
            {
                Product = product.Id,
                UnitAmount = (long)(request.Amount * 100), // Stripe uses cents
                Currency = request.Currency,
                Recurring = new PriceRecurringOptions
                {
                    Interval = request.Interval.ToLower(),
                    IntervalCount = request.IntervalCount
                }
            };
            var priceService = new PriceService();
            var price = await priceService.CreateAsync(priceOptions);

            return new PlanResult
            {
                Success = true,
                PlanId = price.Id,
                ProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                Amount = price.UnitAmount.Value / 100m, // Convert back to decimal
                Currency = price.Currency,
                Interval = price.Recurring.Interval,
                IntervalCount = price.Recurring.IntervalCount
            };
        }
        catch (StripeException e)
        {
            return new PlanResult
            {
                Success = false,
                ErrorMessage = e.Message
            };
        }
    }

    public override async Task<PaymentMethodResult> AddPaymentMethodToCustomerAsync(string customerId, PaymentMethodInfo paymentMethod)
    {
        try
        {
            var options = new PaymentMethodAttachOptions
            {
                Customer = customerId,
            };
            var service = new PaymentMethodService();
            var stripePaymentMethod = await service.AttachAsync(paymentMethod.Details, options);

            // Set this payment method as the default for the customer
            var customerOptions = new CustomerUpdateOptions
            {
                InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = stripePaymentMethod.Id,
                },
            };
            var customerService = new CustomerService();
            await customerService.UpdateAsync(customerId, customerOptions);

            return new PaymentMethodResult
            {
                Success = true,
                PaymentMethodId = stripePaymentMethod.Id,
                Type = stripePaymentMethod.Type,
            };
        }
        catch (StripeException e)
        {
            return new PaymentMethodResult
            {
                Success = false,
                ErrorMessage = e.Message
            };
        }
    }
}