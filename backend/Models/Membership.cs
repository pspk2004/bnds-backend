namespace backend.Models;

public class Membership
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaxAds { get; set; }
    public int MaxBids { get; set; }
    public int MaxFeaturedAds { get; set; }
    public decimal Price { get; set; }
    public int DurationMonths { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}
