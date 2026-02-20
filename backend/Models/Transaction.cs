namespace backend.Models;

public class Transaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty; // Membership, Penalty
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
    public string PaymentMethod { get; set; } = string.Empty; // Card, UPI
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
