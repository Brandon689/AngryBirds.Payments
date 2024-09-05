using AngryBirds.Payments.Core;
using AngryBirds.Payments.Gateways;
using AngryBirds.Payments.Processing;
using Microsoft.Extensions.DependencyInjection;

namespace AngryBirds.Payments.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAngryBirdsPayments(
        this IServiceCollection services,
        GatewayType gatewayType,
        string stripeApiKey = null,
        string paypalClientId = null,
        string paypalClientSecret = null,
        bool paypalUseSandbox = true)
    {
        switch (gatewayType)
        {
            case GatewayType.Sandbox:
                services.AddScoped<IPaymentGateway, SandboxGateway>();
                break;
            case GatewayType.Stripe:
                if (string.IsNullOrEmpty(stripeApiKey))
                {
                    throw new ArgumentException("Stripe API key is required for Stripe gateway", nameof(stripeApiKey));
                }
                services.AddScoped<IPaymentGateway>(sp => new StripeGateway(stripeApiKey));
                break;
            case GatewayType.PayPal:
                if (string.IsNullOrEmpty(paypalClientId) || string.IsNullOrEmpty(paypalClientSecret))
                {
                    throw new ArgumentException("PayPal Client ID and Client Secret are required for PayPal gateway");
                }
                services.AddScoped<IPaymentGateway>(sp => new PayPalGateway(paypalClientId, paypalClientSecret, paypalUseSandbox));
                break;
            default:
                throw new ArgumentException("Unsupported gateway type", nameof(gatewayType));
        }

        services.AddScoped<IPaymentProcessor, PaymentProcessor>();
        return services;
    }
}
