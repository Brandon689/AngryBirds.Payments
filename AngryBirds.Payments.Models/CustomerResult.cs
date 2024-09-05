namespace AngryBirds.Payments.Models;

public class CustomerResult
{
    public bool Success { get; set; }
    public string CustomerId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Description { get; set; }
    public string ErrorMessage { get; set; }
}
