using backend.DTOs;

namespace backend.Services;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(int userId);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto> GetUserByIdAsync(int id);
    Task SuspendUserAsync(SuspendUserDto dto);
    Task UnsuspendUserAsync(int userId);
    Task<TransactionDto> PayPenaltyAsync(int userId, PayPenaltyDto dto);
    Task<List<NotificationDto>> GetNotificationsAsync(int userId);
    Task MarkNotificationReadAsync(int notificationId, int userId);
    Task MarkAllNotificationsReadAsync(int userId);
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<List<TransactionDto>> GetAllTransactionsAsync();
}
