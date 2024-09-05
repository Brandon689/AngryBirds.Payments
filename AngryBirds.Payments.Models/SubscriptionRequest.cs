namespace AngryBirds.Payments.Models;

public class SubscriptionRequest
{
    public string CustomerId { get; set; }
    public string PlanId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Interval { get; set; } // e.g., "month", "year"
    public int IntervalCount { get; set; } // e.g., 1 for monthly, 12 for yearly
}
