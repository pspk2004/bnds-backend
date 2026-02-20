using AutoMapper;
using backend.DTOs;
using backend.Models;
using backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class BidService : IBidService
{
    private readonly IRepository<Bid> _bidRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Notification> _notificationRepo;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public BidService(
        IRepository<Bid> bidRepo,
        IRepository<Product> productRepo,
        IRepository<User> userRepo,
        IRepository<Notification> notificationRepo,
        IEmailService emailService,
        IMapper mapper)
    {
        _bidRepo = bidRepo;
        _productRepo = productRepo;
        _userRepo = userRepo;
        _notificationRepo = notificationRepo;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<BidDto> PlaceBidAsync(int userId, CreateBidDto dto)
    {
        var user = await _userRepo.Query()
            .Include(u => u.Membership)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.IsSuspended)
            throw new InvalidOperationException("Your account is suspended. You cannot place bids.");

        var product = await _productRepo.Query()
            .Include(p => p.Bids)
                .ThenInclude(b => b.User)
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId)
            ?? throw new KeyNotFoundException("Product not found.");

        if (product.IsClosed)
            throw new InvalidOperationException("This auction is closed.");

        if (product.BidEndTime <= DateTime.UtcNow)
            throw new InvalidOperationException("This auction has ended.");

        if (product.SellerId == userId)
            throw new InvalidOperationException("You cannot bid on your own product.");

        if (dto.Amount <= product.CurrentPrice)
            throw new InvalidOperationException($"Bid amount must be higher than the current price of ?{product.CurrentPrice:N2}.");

        // Check membership bid limit
        var membership = user.Membership;
        if (membership != null && membership.MaxBids != -1)
        {
            var userBidCount = await _bidRepo.CountAsync(b => b.UserId == userId && !b.IsWithdrawn);
            if (userBidCount >= membership.MaxBids)
                throw new InvalidOperationException($"You have reached your bid limit ({membership.MaxBids}). Upgrade your membership to place more bids.");
        }

        // Get current highest bidder before this new bid
        var previousHighestBid = product.Bids
            .Where(b => !b.IsWithdrawn)
            .OrderByDescending(b => b.Amount)
            .FirstOrDefault();

        // Create the bid
        var bid = new Bid
        {
            ProductId = dto.ProductId,
            UserId = userId,
            Amount = dto.Amount,
            Time = DateTime.UtcNow
        };

        await _bidRepo.AddAsync(bid);

        // Update product current price
        product.CurrentPrice = dto.Amount;
        _productRepo.Update(product);

        await _bidRepo.SaveChangesAsync();

        // Notify previous highest bidder (if different user)
        if (previousHighestBid != null && previousHighestBid.UserId != userId)
        {
            var previousBidder = previousHighestBid.User;

            var notification = new Notification
            {
                UserId = previousBidder.Id,
                Title = "You've been outbid!",
                Message = $"Someone placed a bid of ?{dto.Amount:N2} on \"{product.Title}\", surpassing your bid of ?{previousHighestBid.Amount:N2}.",
                CreatedAt = DateTime.UtcNow
            };
            await _notificationRepo.AddAsync(notification);
            await _notificationRepo.SaveChangesAsync();

            _ = _emailService.SendOutbidNotificationAsync(
                previousBidder.Email, previousBidder.Name, product.Title, dto.Amount);
        }

        // Return full bid DTO
        var createdBid = await _bidRepo.Query()
            .Include(b => b.Product)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == bid.Id);

        return _mapper.Map<BidDto>(createdBid);
    }

    public async Task WithdrawBidAsync(int bidId, int userId)
    {
        var bid = await _bidRepo.Query()
            .Include(b => b.Product)
            .FirstOrDefaultAsync(b => b.Id == bidId)
            ?? throw new KeyNotFoundException("Bid not found.");

        if (bid.UserId != userId)
            throw new UnauthorizedAccessException("You can only withdraw your own bids.");

        if (bid.IsWithdrawn)
            throw new InvalidOperationException("This bid is already withdrawn.");

        if (bid.Product.IsClosed)
            throw new InvalidOperationException("Cannot withdraw a bid from a closed auction.");

        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        // Withdraw the bid
        bid.IsWithdrawn = true;
        _bidRepo.Update(bid);

        // Increase withdraw count
        user.BidWithdrawCount++;

        // Suspend if > 3 withdrawals
        if (user.BidWithdrawCount > 3)
        {
            user.IsSuspended = true;
            user.SuspensionEndDate = DateTime.UtcNow.AddDays(30);
            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            var notification = new Notification
            {
                UserId = userId,
                Title = "Account Suspended",
                Message = "Your account has been suspended due to exceeding the bid withdrawal limit (3). Pay a penalty to lift the suspension.",
                CreatedAt = DateTime.UtcNow
            };
            await _notificationRepo.AddAsync(notification);
            await _notificationRepo.SaveChangesAsync();

            _ = _emailService.SendSuspensionNotificationAsync(
                user.Email, user.Name, "Exceeded bid withdrawal limit", user.SuspensionEndDate.Value);

            throw new InvalidOperationException("Your account has been suspended due to exceeding the bid withdrawal limit. Pay a penalty to lift the suspension.");
        }

        _userRepo.Update(user);

        // Recalculate current price for the product
        var product = bid.Product;
        var highestActiveBid = await _bidRepo.Query()
            .Where(b => b.ProductId == product.Id && !b.IsWithdrawn)
            .OrderByDescending(b => b.Amount)
            .FirstOrDefaultAsync();

        product.CurrentPrice = highestActiveBid?.Amount ?? product.StartingPrice;
        _productRepo.Update(product);

        await _bidRepo.SaveChangesAsync();
    }

    public async Task<List<BidDto>> GetBidsByProductAsync(int productId)
    {
        var bids = await _bidRepo.Query()
            .Include(b => b.Product)
            .Include(b => b.User)
            .Where(b => b.ProductId == productId)
            .OrderByDescending(b => b.Amount)
            .ToListAsync();

        return _mapper.Map<List<BidDto>>(bids);
    }

    public async Task<List<BidDto>> GetBidsByUserAsync(int userId)
    {
        var bids = await _bidRepo.Query()
            .Include(b => b.Product)
            .Include(b => b.User)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.Time)
            .ToListAsync();

        return _mapper.Map<List<BidDto>>(bids);
    }
}
