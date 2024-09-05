using AngryBirds.Payments.Core;
using AngryBirds.Payments.DependencyInjection;
using AngryBirds.Payments.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddUserSecrets<Program>();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddAngryBirdsPayments(
            GatewayType.Stripe,
            stripeApiKey: context.Configuration["Stripe:ApiKey"]
        );
    })
    .Build();

// Get the payment processor
var paymentProcessor = host.Services.GetRequiredService<IPaymentProcessor>();

// Create a sample payment request
var paymentRequest = new PaymentRequest
{
    Amount = 10.00m,
    Currency = "usd",
    PaymentMethod = new PaymentMethodInfo
    {
        Type = "card",
        Details = "pm_card_visa" // Stripe test card token
    },
    Customer = new Customer { Name = "Test User", Email = "test@example.com" },
    Description = "Test Stripe payment"
};

Console.WriteLine("Processing payment...");
var paymentResult = await paymentProcessor.ProcessPaymentAsync(paymentRequest);

// Display the payment result
Console.WriteLine($"Payment {(paymentResult.Success ? "succeeded" : "failed")}");
Console.WriteLine($"Transaction ID: {paymentResult.TransactionId}");
Console.WriteLine($"Status: {paymentResult.Status}");
Console.WriteLine($"Amount Processed: {paymentResult.AmountProcessed} {paymentResult.Currency}");
Console.WriteLine($"Timestamp: {paymentResult.Timestamp}");
if (!paymentResult.Success)
{
    Console.WriteLine($"Error: {paymentResult.ErrorMessage}");
}

// If payment succeeded, get transaction details
if (paymentResult.Success)
{
    Console.WriteLine("\nRetrieving transaction details...");
    var transactionDetails = await paymentProcessor.GetTransactionDetailsAsync(paymentResult.TransactionId);

    // Display the transaction details
    Console.WriteLine("Transaction Details:");
    Console.WriteLine($"Transaction ID: {transactionDetails.TransactionId}");
    Console.WriteLine($"Amount: {transactionDetails.Amount} {transactionDetails.Currency}");
    Console.WriteLine($"Status: {transactionDetails.Status}");
    Console.WriteLine($"Timestamp: {transactionDetails.Timestamp}");
    Console.WriteLine($"Payment Method Type: {transactionDetails.PaymentMethodType}");
    Console.WriteLine($"Customer Name: {transactionDetails.CustomerName}");
    Console.WriteLine($"Customer Email: {transactionDetails.CustomerEmail}");
    Console.WriteLine($"Description: {transactionDetails.Description}");

    // Process a refund
    Console.WriteLine("\nProcessing refund...");
    var refundRequest = new RefundRequest
    {
        TransactionId = paymentResult.TransactionId,
        Amount = paymentResult.AmountProcessed,
        Currency = paymentResult.Currency,
        Reason = "customer_requested"
    };

    var refundResult = await paymentProcessor.ProcessRefundAsync(refundRequest);

    // Display the refund result
    Console.WriteLine($"Refund {(refundResult.Success ? "succeeded" : "failed")}");
    Console.WriteLine($"Refund ID: {refundResult.RefundId}");
    Console.WriteLine($"Refunded Amount: {refundResult.RefundedAmount}");
    Console.WriteLine($"Timestamp: {refundResult.Timestamp}");
    if (!refundResult.Success)
    {
        Console.WriteLine($"Error: {refundResult.ErrorMessage}");
    }

    // Create a customer
    Console.WriteLine("\nCreating customer...");
    var customerRequest = new CustomerRequest
    {
        Name = "John Doe",
        Email = "john.doe@example.com",
        Description = "Test customer"
    };

    var createCustomerResult = await paymentProcessor.CreateCustomerAsync(customerRequest);

    // Display the customer creation result
    Console.WriteLine($"Customer creation {(createCustomerResult.Success ? "succeeded" : "failed")}");
    Console.WriteLine($"Customer ID: {createCustomerResult.CustomerId}");
    Console.WriteLine($"Name: {createCustomerResult.Name}");
    Console.WriteLine($"Email: {createCustomerResult.Email}");
    Console.WriteLine($"Description: {createCustomerResult.Description}");
    if (!createCustomerResult.Success)
    {
        Console.WriteLine($"Error: {createCustomerResult.ErrorMessage}");
    }

    if (createCustomerResult.Success)
    {
        // Add a payment method to the customer
        Console.WriteLine("\nAdding payment method to customer...");
        var paymentMethodInfo = new PaymentMethodInfo
        {
            Type = "card",
            Details = "pm_card_visa" // This is a test card token provided by Stripe
        };

        var addPaymentMethodResult = await paymentProcessor.AddPaymentMethodToCustomerAsync(createCustomerResult.CustomerId, paymentMethodInfo);

        // Display the result of adding the payment method
        Console.WriteLine($"Adding payment method {(addPaymentMethodResult.Success ? "succeeded" : "failed")}");
        Console.WriteLine($"Payment Method ID: {addPaymentMethodResult.PaymentMethodId}");
        Console.WriteLine($"Type: {addPaymentMethodResult.Type}");
        if (!addPaymentMethodResult.Success)
        {
            Console.WriteLine($"Error: {addPaymentMethodResult.ErrorMessage}");
        }

        // Retrieve the customer
        Console.WriteLine("\nRetrieving customer...");
        var getCustomerResult = await paymentProcessor.GetCustomerAsync(createCustomerResult.CustomerId);

        // Display the retrieved customer details
        Console.WriteLine($"Customer retrieval {(getCustomerResult.Success ? "succeeded" : "failed")}");
        Console.WriteLine($"Customer ID: {getCustomerResult.CustomerId}");
        Console.WriteLine($"Name: {getCustomerResult.Name}");
        Console.WriteLine($"Email: {getCustomerResult.Email}");
        Console.WriteLine($"Description: {getCustomerResult.Description}");
        if (!getCustomerResult.Success)
        {
            Console.WriteLine($"Error: {getCustomerResult.ErrorMessage}");
        }

        // Update the customer
        Console.WriteLine("\nUpdating customer...");
        var updateCustomerRequest = new CustomerRequest
        {
            Name = "John Updated Doe",
            Email = "john.updated@example.com",
            Description = "Updated test customer"
        };

        var updateCustomerResult = await paymentProcessor.UpdateCustomerAsync(createCustomerResult.CustomerId, updateCustomerRequest);

        // Display the customer update result
        Console.WriteLine($"Customer update {(updateCustomerResult.Success ? "succeeded" : "failed")}");
        Console.WriteLine($"Customer ID: {updateCustomerResult.CustomerId}");
        Console.WriteLine($"Name: {updateCustomerResult.Name}");
        Console.WriteLine($"Email: {updateCustomerResult.Email}");
        Console.WriteLine($"Description: {updateCustomerResult.Description}");
        if (!updateCustomerResult.Success)
        {
            Console.WriteLine($"Error: {updateCustomerResult.ErrorMessage}");
        }

        // Create a plan
        Console.WriteLine("\nCreating plan...");
        var planRequest = new PlanRequest
        {
            Name = "Premium Plan",
            Description = "Monthly premium subscription",
            Amount = 19.99m,
            Currency = "usd",
            Interval = "month",
            IntervalCount = 1
        };

        var createPlanResult = await paymentProcessor.CreatePlanAsync(planRequest);

        // Display the plan creation result
        Console.WriteLine($"Plan creation {(createPlanResult.Success ? "succeeded" : "failed")}");
        Console.WriteLine($"Plan ID: {createPlanResult.PlanId}");
        Console.WriteLine($"Product ID: {createPlanResult.ProductId}");
        Console.WriteLine($"Name: {createPlanResult.Name}");
        Console.WriteLine($"Description: {createPlanResult.Description}");
        Console.WriteLine($"Amount: {createPlanResult.Amount} {createPlanResult.Currency}");
        Console.WriteLine($"Interval: {createPlanResult.Interval}");
        Console.WriteLine($"Interval Count: {createPlanResult.IntervalCount}");
        if (!createPlanResult.Success)
        {
            Console.WriteLine($"Error: {createPlanResult.ErrorMessage}");
        }

        // Create a subscription
        Console.WriteLine("\nCreating subscription...");
        var subscriptionRequest = new SubscriptionRequest
        {
            CustomerId = createCustomerResult.CustomerId,
            PlanId = createPlanResult.PlanId,
            Amount = 19.99m,
            Currency = "usd",
            Interval = "month",
            IntervalCount = 1
        };

        var subscriptionResult = await paymentProcessor.CreateSubscriptionAsync(subscriptionRequest);

        // Display the subscription result
        Console.WriteLine($"Subscription creation {(subscriptionResult.Success ? "succeeded" : "failed")}");
        Console.WriteLine($"Subscription ID: {subscriptionResult.SubscriptionId}");
        Console.WriteLine($"Status: {subscriptionResult.Status}");
        Console.WriteLine($"Start Date: {subscriptionResult.StartDate}");
        Console.WriteLine($"End Date: {subscriptionResult.EndDate}");
        if (!subscriptionResult.Success)
        {
            Console.WriteLine($"Error: {subscriptionResult.ErrorMessage}");
        }

        // If subscription creation succeeded, cancel it
        if (subscriptionResult.Success)
        {
            Console.WriteLine("\nCancelling subscription...");
            var cancelResult = await paymentProcessor.CancelSubscriptionAsync(subscriptionResult.SubscriptionId);

            // Display the cancellation result
            Console.WriteLine($"Subscription cancellation {(cancelResult.Success ? "succeeded" : "failed")}");
            Console.WriteLine($"Subscription ID: {cancelResult.SubscriptionId}");
            Console.WriteLine($"Status: {cancelResult.Status}");
            Console.WriteLine($"End Date: {cancelResult.EndDate}");
            if (!cancelResult.Success)
            {
                Console.WriteLine($"Error: {cancelResult.ErrorMessage}");
            }
        }
    }
}

await host.RunAsync();



//using AngryBirds.Payments.Core;
//using AngryBirds.Payments.DependencyInjection;
//using AngryBirds.Payments.Models;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;

//// Setup Dependency Injection using HostBuilder
//var builder = Host.CreateApplicationBuilder(args);

//// Read from configuration or environment variable
//var gatewayType = Environment.GetEnvironmentVariable("PAYMENT_GATEWAY_TYPE") == "Stripe"
//    ? GatewayType.Stripe
//    : GatewayType.Sandbox;

//var stripeApiKey = Environment.GetEnvironmentVariable("STRIPE_API_KEY");

//builder.Services.AddAngryBirdsPayments(gatewayType, stripeApiKey);

//var host = builder.Build();

//// Get the payment processor
//var paymentProcessor = host.Services.GetRequiredService<IPaymentProcessor>();

//// Create a sample payment request
//var paymentRequest = new PaymentRequest
//{
//    Amount = 100.00m,
//    Currency = "USD",
//    PaymentMethod = new PaymentMethod { Type = "CreditCard", Details = "4111111111111111" },
//    Customer = new Customer { Name = "John Doe", Email = "john@example.com" },
//    BillingAddress = new Address { Country = "US", PostalCode = "12345" },
//    Description = "Test payment"
//};

//// Process the payment
//Console.WriteLine("Processing payment...");
//var result = await paymentProcessor.ProcessPaymentAsync(paymentRequest);

//// Display the result
//Console.WriteLine($"Payment {(result.Success ? "succeeded" : "failed")}");
//Console.WriteLine($"Transaction ID: {result.TransactionId}");
//Console.WriteLine($"Status: {result.Status}");
//Console.WriteLine($"Amount Processed: {result.AmountProcessed} {result.Currency}");
//if (!result.Success)
//{
//    Console.WriteLine($"Error: {result.ErrorMessage}");
//}

//await host.RunAsync();
