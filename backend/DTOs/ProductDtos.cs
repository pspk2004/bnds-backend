namespace backend.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime BidEndTime { get; set; }
    public bool IsClosed { get; set; }
    public int? WinnerId { get; set; }
    public string? WinnerName { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalBids { get; set; }
}

public class CreateProductDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public decimal StartingPrice { get; set; }
    public DateTime BidEndTime { get; set; }
}

public class UpdateProductDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
}

public class ProductFilterDto
{
    public int? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? EndingSoon { get; set; }
    public string? SortBy { get; set; } // priceAsc, priceDesc, newest, endingSoon
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
