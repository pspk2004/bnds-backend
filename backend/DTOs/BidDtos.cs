namespace backend.DTOs;

public class BidDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Time { get; set; }
    public bool IsWithdrawn { get; set; }
}

public class CreateBidDto
{
    public int ProductId { get; set; }
    public decimal Amount { get; set; }
}
