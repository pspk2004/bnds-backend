namespace backend.Models;

public class Bid
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Time { get; set; } = DateTime.UtcNow;
    public bool IsWithdrawn { get; set; }

    public Product Product { get; set; } = null!;
    public User User { get; set; } = null!;
}
