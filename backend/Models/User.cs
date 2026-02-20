namespace backend.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public int? MembershipId { get; set; }
    public DateTime? MembershipExpiry { get; set; }
    public int BidWithdrawCount { get; set; }
    public bool IsSuspended { get; set; }
    public DateTime? SuspensionEndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Membership? Membership { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Product> WonProducts { get; set; } = new List<Product>();
}
