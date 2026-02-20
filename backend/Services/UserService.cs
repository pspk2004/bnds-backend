using AutoMapper;
using backend.DTOs;
using backend.Models;
using backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<Bid> _bidRepo;
    private readonly IRepository<Transaction> _transactionRepo;
    private readonly IRepository<Notification> _notificationRepo;
    private readonly IRepository<Category> _categoryRepo;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public UserService(
        IRepository<User> userRepo,
        IRepository<Product> productRepo,
        IRepository<Bid> bidRepo,
        IRepository<Transaction> transactionRepo,
        IRepository<Notification> notificationRepo,
        IRepository<Category> categoryRepo,
        IEmailService emailService,
        IMapper mapper)
    {
        _userRepo = userRepo;
        _productRepo = productRepo;
        _bidRepo = bidRepo;
        _transactionRepo = transactionRepo;
        _notificationRepo = notificationRepo;
        _categoryRepo = categoryRepo;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _userRepo.Query()
            .Include(u => u.Membership)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");

        var activeAdsCount = await _productRepo.CountAsync(p => p.SellerId == userId && !p.IsClosed);
        var activeBidsCount = await _bidRepo.CountAsync(b => b.UserId == userId && !b.IsWithdrawn);
        var featuredAdsCount = await _productRepo.CountAsync(p => p.SellerId == userId && p.IsFeatured && !p.IsClosed);

        return new UserProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            MembershipName = user.Membership?.Name,
            MembershipExpiry = user.MembershipExpiry,
            BidWithdrawCount = user.BidWithdrawCount,
            IsSuspended = user.IsSuspended,
            ActiveAdsCount = activeAdsCount,
            ActiveBidsCount = activeBidsCount,
            FeaturedAdsCount = featuredAdsCount,
            MaxAds = user.Membership?.MaxAds ?? 1,
            MaxBids = user.Membership?.MaxBids ?? 5,
            MaxFeaturedAds = user.Membership?.MaxFeaturedAds ?? 0
        };
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepo.Query()
            .Include(u => u.Membership)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<UserDto>>(users);
    }

    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var user = await _userRepo.Query()
            .Include(u => u.Membership)
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new KeyNotFoundException("User not found.");

        return _mapper.Map<UserDto>(user);
    }

    public async Task SuspendUserAsync(SuspendUserDto dto)
    {
        var user = await _userRepo.GetByIdAsync(dto.UserId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.Role == "Admin")
            throw new InvalidOperationException("Cannot suspend an admin.");

        user.IsSuspended = true;
        user.SuspensionEndDate = DateTime.UtcNow.AddDays(dto.SuspensionDays);
        _userRepo.Update(user);

        var notification = new Notification
        {
            UserId = user.Id,
            Title = "Account Suspended",
            Message = $"Your account has been suspended for {dto.SuspensionDays} days. Reason: {dto.Reason}",
            CreatedAt = DateTime.UtcNow
        };
        await _notificationRepo.AddAsync(notification);
        await _userRepo.SaveChangesAsync();

        _ = _emailService.SendSuspensionNotificationAsync(
            user.Email, user.Name, dto.Reason, user.SuspensionEndDate.Value);
    }

    public async Task UnsuspendUserAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (!user.IsSuspended)
            throw new InvalidOperationException("User is not suspended.");

        user.IsSuspended = false;
        user.SuspensionEndDate = null;
        _userRepo.Update(user);

        var notification = new Notification
        {
            UserId = user.Id,
            Title = "Suspension Lifted",
            Message = "Your account suspension has been lifted by an administrator.",
            CreatedAt = DateTime.UtcNow
        };
        await _notificationRepo.AddAsync(notification);
        await _userRepo.SaveChangesAsync();
    }

    public async Task<TransactionDto> PayPenaltyAsync(int userId, PayPenaltyDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (!user.IsSuspended)
            throw new InvalidOperationException("Your account is not suspended. No penalty to pay.");

        var penaltyAmount = 500m;

        var transaction = new Transaction
        {
            UserId = userId,
            Amount = penaltyAmount,
            Type = "Penalty",
            Status = "Completed",
            PaymentMethod = dto.PaymentMethod,
            CreatedAt = DateTime.UtcNow
        };

        await _transactionRepo.AddAsync(transaction);

        user.IsSuspended = false;
        user.SuspensionEndDate = null;
        user.BidWithdrawCount = 0;
        _userRepo.Update(user);

        var notification = new Notification
        {
            UserId = userId,
            Title = "Penalty Paid - Suspension Lifted",
            Message = $"Your penalty of ?{penaltyAmount:N2} has been paid. Your account is now active.",
            CreatedAt = DateTime.UtcNow
        };
        await _notificationRepo.AddAsync(notification);

        await _transactionRepo.SaveChangesAsync();

        var createdTransaction = await _transactionRepo.Query()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == transaction.Id);

        return _mapper.Map<TransactionDto>(createdTransaction);
    }

    public async Task<List<NotificationDto>> GetNotificationsAsync(int userId)
    {
        var notifications = await _notificationRepo.Query()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<NotificationDto>>(notifications);
    }

    public async Task MarkNotificationReadAsync(int notificationId, int userId)
    {
        var notification = await _notificationRepo.GetByIdAsync(notificationId)
            ?? throw new KeyNotFoundException("Notification not found.");

        if (notification.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        notification.IsRead = true;
        _notificationRepo.Update(notification);
        await _notificationRepo.SaveChangesAsync();
    }

    public async Task MarkAllNotificationsReadAsync(int userId)
    {
        var notifications = await _notificationRepo.FindAsync(n => n.UserId == userId && !n.IsRead);
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            _notificationRepo.Update(notification);
        }
        await _notificationRepo.SaveChangesAsync();
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var totalUsers = await _userRepo.CountAsync(u => u.Role == "User");
        var totalProducts = await _productRepo.CountAsync(_ => true);
        var activeAuctions = await _productRepo.CountAsync(p => !p.IsClosed);
        var closedAuctions = await _productRepo.CountAsync(p => p.IsClosed);
        var totalBids = await _bidRepo.CountAsync(_ => true);
        var suspendedUsers = await _userRepo.CountAsync(u => u.IsSuspended);
        var featuredProducts = await _productRepo.CountAsync(p => p.IsFeatured);

        var totalRevenue = await _transactionRepo.Query()
            .Where(t => t.Status == "Completed")
            .SumAsync(t => t.Amount);

        var categoryStats = await _categoryRepo.Query()
            .Select(c => new CategoryStatsDto
            {
                CategoryName = c.Name,
                ProductCount = c.Products.Count
            })
            .ToListAsync();

        var recentTransactions = await _transactionRepo.Query()
            .Include(t => t.User)
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .Select(t => new RecentTransactionDto
            {
                Id = t.Id,
                UserName = t.User.Name,
                Amount = t.Amount,
                Type = t.Type,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return new DashboardStatsDto
        {
            TotalUsers = totalUsers,
            TotalProducts = totalProducts,
            ActiveAuctions = activeAuctions,
            ClosedAuctions = closedAuctions,
            TotalBids = totalBids,
            TotalRevenue = totalRevenue,
            SuspendedUsers = suspendedUsers,
            FeaturedProducts = featuredProducts,
            CategoryStats = categoryStats,
            RecentTransactions = recentTransactions
        };
    }

    public async Task<List<TransactionDto>> GetAllTransactionsAsync()
    {
        var transactions = await _transactionRepo.Query()
            .Include(t => t.User)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<TransactionDto>>(transactions);
    }
}
