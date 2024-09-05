namespace AngryBirds.Payments.Models;

public class SubscriptionResult
{
    public bool Success { get; set; }
    public string SubscriptionId { get; set; }
    public string Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string ErrorMessage { get; set; }
}