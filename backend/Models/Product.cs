namespace backend.Models;

public class Product
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int SellerId { get; set; }
    public decimal StartingPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime BidEndTime { get; set; }
    public bool IsClosed { get; set; }
    public int? WinnerId { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Category Category { get; set; } = null!;
    public User Seller { get; set; } = null!;
    public User? Winner { get; set; }
    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
}
