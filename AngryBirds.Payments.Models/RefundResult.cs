namespace AngryBirds.Payments.Models;

public class RefundResult
{
    public bool Success { get; set; }
    public string RefundId { get; set; }
    public string ErrorMessage { get; set; }
    public decimal RefundedAmount { get; set; }
    public DateTime Timestamp { get; set; }

    public RefundResult()
    {
        Timestamp = DateTime.UtcNow;
    }
}
