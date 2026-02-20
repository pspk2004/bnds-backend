namespace backend.Services;

public interface IEmailService
{
    Task SendOutbidNotificationAsync(string toEmail, string toName, string productTitle, decimal newBidAmount);
    Task SendAuctionWonNotificationAsync(string toEmail, string toName, string productTitle, decimal winningAmount);
    Task SendSuspensionNotificationAsync(string toEmail, string toName, string reason, DateTime suspensionEndDate);
    Task SendMembershipActivatedNotificationAsync(string toEmail, string toName, string membershipName, DateTime expiryDate);
}
