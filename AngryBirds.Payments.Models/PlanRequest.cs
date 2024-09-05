namespace AngryBirds.Payments.Models;

public class PlanRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Interval { get; set; } // "day", "week", "month" or "year"
    public int IntervalCount { get; set; }
}
