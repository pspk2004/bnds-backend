namespace backend.DTOs;

public class MembershipDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaxAds { get; set; }
    public int MaxBids { get; set; }
    public int MaxFeaturedAds { get; set; }
    public decimal Price { get; set; }
    public int DurationMonths { get; set; }
}

public class PurchaseMembershipDto
{
    public int MembershipId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // Card, UPI
}
