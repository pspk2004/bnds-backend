namespace backend.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalProducts { get; set; }
    public int ActiveAuctions { get; set; }
    public int ClosedAuctions { get; set; }
    public int TotalBids { get; set; }
    public decimal TotalRevenue { get; set; }
    public int SuspendedUsers { get; set; }
    public int FeaturedProducts { get; set; }
    public List<CategoryStatsDto> CategoryStats { get; set; } = new();
    public List<RecentTransactionDto> RecentTransactions { get; set; } = new();
}

public class CategoryStatsDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}

public class RecentTransactionDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
    {
        return new ApiResponse<T> { Success = true, Message = message, Data = data };
    }

    public static ApiResponse<T> FailResponse(string message)
    {
        return new ApiResponse<T> { Success = false, Message = message };
    }
}
