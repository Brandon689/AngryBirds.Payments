namespace AngryBirds.Payments.Models;

public class PlanResult
{
    public bool Success { get; set; }
    public string PlanId { get; set; }
    public string ProductId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Interval { get; set; }
    public long IntervalCount { get; set; }
    public string ErrorMessage { get; set; }
}