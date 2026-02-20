using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.BackgroundServices;

public class AuctionClosingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuctionClosingService> _logger;

    public AuctionClosingService(IServiceProvider serviceProvider, ILogger<AuctionClosingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auction Closing Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredAuctionsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing expired auctions.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ProcessExpiredAuctionsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var expiredProducts = await context.Products
            .Include(p => p.Bids)
                .ThenInclude(b => b.User)
            .Include(p => p.Seller)
            .Where(p => !p.IsClosed && p.BidEndTime <= DateTime.UtcNow)
            .ToListAsync();

        if (expiredProducts.Count == 0)
            return;

        _logger.LogInformation("Found {Count} expired auctions to close.", expiredProducts.Count);

        foreach (var product in expiredProducts)
        {
            product.IsClosed = true;

            var highestBid = product.Bids
                .Where(b => !b.IsWithdrawn)
                .OrderByDescending(b => b.Amount)
                .FirstOrDefault();

            if (highestBid != null)
            {
                product.WinnerId = highestBid.UserId;
                product.CurrentPrice = highestBid.Amount;

                // Create notification for winner
                var winnerNotification = new Notification
                {
                    UserId = highestBid.UserId,
                    Title = "Auction Won!",
                    Message = $"Congratulations! You won the auction for \"{product.Title}\" with a bid of ?{highestBid.Amount:N2}.",
                    CreatedAt = DateTime.UtcNow
                };
                context.Notifications.Add(winnerNotification);

                // Create notification for seller
                var sellerNotification = new Notification
                {
                    UserId = product.SellerId,
                    Title = "Auction Closed",
                    Message = $"Your auction for \"{product.Title}\" has ended. Winner: {highestBid.User.Name} with ?{highestBid.Amount:N2}.",
                    CreatedAt = DateTime.UtcNow
                };
                context.Notifications.Add(sellerNotification);

                // Send email to winner
                _ = emailService.SendAuctionWonNotificationAsync(
                    highestBid.User.Email, highestBid.User.Name, product.Title, highestBid.Amount);

                _logger.LogInformation("Auction {ProductId} closed. Winner: {WinnerId}, Amount: {Amount}",
                    product.Id, highestBid.UserId, highestBid.Amount);
            }
            else
            {
                // No bids - notify seller
                var sellerNotification = new Notification
                {
                    UserId = product.SellerId,
                    Title = "Auction Closed - No Bids",
                    Message = $"Your auction for \"{product.Title}\" has ended without any bids.",
                    CreatedAt = DateTime.UtcNow
                };
                context.Notifications.Add(sellerNotification);

                _logger.LogInformation("Auction {ProductId} closed with no bids.", product.Id);
            }
        }

        await context.SaveChangesAsync();
    }
}
