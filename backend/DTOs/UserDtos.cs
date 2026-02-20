namespace backend.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? MembershipId { get; set; }
    public string? MembershipName { get; set; }
    public DateTime? MembershipExpiry { get; set; }
    public int BidWithdrawCount { get; set; }
    public bool IsSuspended { get; set; }
    public DateTime? SuspensionEndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? MembershipName { get; set; }
    public DateTime? MembershipExpiry { get; set; }
    public int BidWithdrawCount { get; set; }
    public bool IsSuspended { get; set; }
    public int ActiveAdsCount { get; set; }
    public int ActiveBidsCount { get; set; }
    public int FeaturedAdsCount { get; set; }
    public int MaxAds { get; set; }
    public int MaxBids { get; set; }
    public int MaxFeaturedAds { get; set; }
}

public class SuspendUserDto
{
    public int UserId { get; set; }
    public int SuspensionDays { get; set; }
    public string Reason { get; set; } = string.Empty;
}
