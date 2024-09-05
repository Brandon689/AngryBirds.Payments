using AngryBirds.Payments.Exceptions;
using AngryBirds.Payments.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AngryBirds.Payments.Gateways;

public class PayPalGateway : BasePaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly bool _isSandbox;

    public PayPalGateway(string clientId, string clientSecret, bool isSandbox)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _isSandbox = isSandbox;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_isSandbox
                ? "https://api-m.sandbox.paypal.com"
                : "https://api-m.paypal.com")
        };
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/v1/oauth2/token");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"))
        );

        var content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
        request.Content = content;

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
        return tokenResponse.GetProperty("access_token").GetString();
    }

    public override async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync();

            var paymentRequest = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                        new
                        {
                            amount = new
                            {
                                currency_code = request.Currency,
                                value = request.Amount.ToString("0.00")
                            }
                        }
                    }
            };

            var jsonContent = JsonSerializer.Serialize(paymentRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.PostAsync("/v2/checkout/orders", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            var paymentResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (response.IsSuccessStatusCode)
            {
                return new PaymentResult
                {
                    Success = true,
                    TransactionId = paymentResponse.GetProperty("id").GetString(),
                    Status = paymentResponse.GetProperty("status").GetString(),
                    AmountProcessed = request.Amount,
                    Currency = request.Currency,
                    Timestamp = DateTime.UtcNow
                };
            }
            else
            {
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = paymentResponse.GetProperty("message").GetString(),
                    Timestamp = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            throw new PaymentException("An error occurred while processing the PayPal payment", "PAYPAL_ERROR", ex);
        }
    }

    public override async Task<RefundResult> ProcessRefundAsync(RefundRequest request)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync();

            var refundRequest = new
            {
                amount = new
                {
                    currency_code = request.Currency,
                    value = request.Amount.ToString("0.00")
                }
            };

            var jsonContent = JsonSerializer.Serialize(refundRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.PostAsync($"/v2/payments/captures/{request.TransactionId}/refund", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            var refundResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (response.IsSuccessStatusCode)
            {
                return new RefundResult
                {
                    Success = true,
                    RefundId = refundResponse.GetProperty("id").GetString(),
                    RefundedAmount = request.Amount,
                    Timestamp = DateTime.UtcNow
                };
            }
            else
            {
                return new RefundResult
                {
                    Success = false,
                    ErrorMessage = refundResponse.GetProperty("message").GetString(),
                    Timestamp = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            throw new PaymentException("An error occurred while processing the PayPal refund", "PAYPAL_REFUND_ERROR", ex);
        }
    }

    public override async Task<TransactionDetails> GetTransactionDetailsAsync(string transactionId)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync($"/v2/checkout/orders/{transactionId}");

            var responseContent = await response.Content.ReadAsStringAsync();
            var orderDetails = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (response.IsSuccessStatusCode)
            {
                var purchaseUnit = orderDetails.GetProperty("purchase_units")[0];
                var amount = purchaseUnit.GetProperty("amount");

                return new TransactionDetails
                {
                    TransactionId = orderDetails.GetProperty("id").GetString(),
                    Amount = decimal.Parse(amount.GetProperty("value").GetString()),
                    Currency = amount.GetProperty("currency_code").GetString(),
                    Status = orderDetails.GetProperty("status").GetString(),
                    Timestamp = DateTime.Parse(orderDetails.GetProperty("create_time").GetString()),
                    PaymentMethodType = "paypal", // PayPal doesn't provide specific payment method type in this API
                    CustomerName = orderDetails.TryGetProperty("payer", out var payer)
                        ? $"{payer.GetProperty("name").GetProperty("given_name").GetString()} {payer.GetProperty("name").GetProperty("surname").GetString()}"
                        : null,
                    CustomerEmail = orderDetails.TryGetProperty("payer", out var payerWithEmail)
                        ? payerWithEmail.GetProperty("email_address").GetString()
                        : null,
                    Description = purchaseUnit.TryGetProperty("description", out var description)
                        ? description.GetString()
                        : null
                };
            }
            else
            {
                throw new PaymentException($"Failed to retrieve transaction details: {orderDetails.GetProperty("message").GetString()}", "PAYPAL_ERROR");
            }
        }
        catch (Exception ex)
        {
            throw new PaymentException("An error occurred while retrieving the PayPal transaction details", "PAYPAL_ERROR", ex);
        }
    }

    public override async Task<SubscriptionResult> CreateSubscriptionAsync(SubscriptionRequest request)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync();

            var subscriptionRequest = new
            {
                plan_id = request.PlanId,
                subscriber = new
                {
                    name = new
                    {
                        given_name = request.CustomerId // In a real scenario, you'd split the name
                    },
                    email_address = "subscriber@example.com" // You'd need to pass this in the SubscriptionRequest
                },
                application_context = new
                {
                    brand_name = "My Brand",
                    locale = "en-US",
                    shipping_preference = "SET_PROVIDED_ADDRESS",
                    user_action = "SUBSCRIBE_NOW",
                    payment_method = new
                    {
                        payer_selected = "PAYPAL",
                        payee_preferred = "IMMEDIATE_PAYMENT_REQUIRED"
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(subscriptionRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.PostAsync("/v1/billing/subscriptions", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            var subscriptionResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (response.IsSuccessStatusCode)
            {
                return new SubscriptionResult
                {
                    Success = true,
                    SubscriptionId = subscriptionResponse.GetProperty("id").GetString(),
                    Status = subscriptionResponse.GetProperty("status").GetString(),
                    StartDate = DateTime.Parse(subscriptionResponse.GetProperty("start_time").GetString()),
                    // PayPal doesn't provide an end date for active subscriptions
                };
            }
            else
            {
                return new SubscriptionResult
                {
                    Success = false,
                    ErrorMessage = subscriptionResponse.GetProperty("message").GetString(),
                };
            }
        }
        catch (Exception ex)
        {
            throw new PaymentException("An error occurred while creating the PayPal subscription", "PAYPAL_ERROR", ex);
        }
    }

    public override async Task<SubscriptionResult> CancelSubscriptionAsync(string subscriptionId)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync();

            var cancelRequest = new
            {
                reason = "Not satisfied with the service"
            };

            var jsonContent = JsonSerializer.Serialize(cancelRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.PostAsync($"/v1/billing/subscriptions/{subscriptionId}/cancel", content);

            if (response.IsSuccessStatusCode)
            {
                // For successful cancellation, PayPal returns an empty response body
                return new SubscriptionResult
                {
                    Success = true,
                    SubscriptionId = subscriptionId,
                    Status = "cancelled",
                    EndDate = DateTime.UtcNow
                };
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                return new SubscriptionResult
                {
                    Success = false,
                    ErrorMessage = errorResponse.GetProperty("message").GetString(),
                };
            }
        }
        catch (Exception ex)
        {
            throw new PaymentException("An error occurred while cancelling the PayPal subscription", "PAYPAL_ERROR", ex);
        }
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

    public override Task<PaymentMethodResult> AddPaymentMethodToCustomerAsync(string customerId, PaymentMethodInfo paymentMethod)
    {
        throw new NotImplementedException("AddPaymentMethodToCustomerAsync is not implemented for PayPalGateway");
    }
    //public override Task<SubscriptionResult> CreateSubscriptionAsync(SubscriptionRequest request)
    //{
    //    throw new NotImplementedException("CreateSubscriptionAsync is not implemented for PayPalGateway");
    //}

    //public override Task<SubscriptionResult> CancelSubscriptionAsync(string subscriptionId)
    //{
    //    throw new NotImplementedException("CancelSubscriptionAsync is not implemented for PayPalGateway");
    //}
}