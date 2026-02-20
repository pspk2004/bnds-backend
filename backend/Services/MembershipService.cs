using AutoMapper;
using backend.DTOs;
using backend.Models;
using backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class MembershipService : IMembershipService
{
    private readonly IRepository<Membership> _membershipRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Transaction> _transactionRepo;
    private readonly IRepository<Notification> _notificationRepo;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public MembershipService(
        IRepository<Membership> membershipRepo,
        IRepository<User> userRepo,
        IRepository<Transaction> transactionRepo,
        IRepository<Notification> notificationRepo,
        IEmailService emailService,
        IMapper mapper)
    {
        _membershipRepo = membershipRepo;
        _userRepo = userRepo;
        _transactionRepo = transactionRepo;
        _notificationRepo = notificationRepo;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<List<MembershipDto>> GetAllMembershipsAsync()
    {
        var memberships = await _membershipRepo.GetAllAsync();
        return _mapper.Map<List<MembershipDto>>(memberships);
    }

    public async Task<MembershipDto> GetMembershipByIdAsync(int id)
    {
        var membership = await _membershipRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Membership not found.");
        return _mapper.Map<MembershipDto>(membership);
    }

    public async Task<TransactionDto> PurchaseMembershipAsync(int userId, PurchaseMembershipDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var membership = await _membershipRepo.GetByIdAsync(dto.MembershipId)
            ?? throw new KeyNotFoundException("Membership plan not found.");

        if (membership.Price == 0)
            throw new InvalidOperationException("Free membership cannot be purchased. It is assigned by default.");

        // Create transaction
        var transaction = new Transaction
        {
            UserId = userId,
            Amount = membership.Price,
            Type = "Membership",
            Status = "Completed",
            PaymentMethod = dto.PaymentMethod,
            CreatedAt = DateTime.UtcNow
        };

        await _transactionRepo.AddAsync(transaction);

        // Activate membership
        user.MembershipId = membership.Id;
        user.MembershipExpiry = DateTime.UtcNow.AddMonths(membership.DurationMonths);
        _userRepo.Update(user);

        // Create notification
        var notification = new Notification
        {
            UserId = userId,
            Title = "Membership Activated",
            Message = $"Your {membership.Name} membership has been activated and is valid until {user.MembershipExpiry:MMMM dd, yyyy}.",
            CreatedAt = DateTime.UtcNow
        };
        await _notificationRepo.AddAsync(notification);

        await _transactionRepo.SaveChangesAsync();

        _ = _emailService.SendMembershipActivatedNotificationAsync(
            user.Email, user.Name, membership.Name, user.MembershipExpiry.Value);

        var createdTransaction = await _transactionRepo.Query()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == transaction.Id);

        return _mapper.Map<TransactionDto>(createdTransaction);
    }
}
