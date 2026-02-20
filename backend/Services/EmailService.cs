using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace backend.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendOutbidNotificationAsync(string toEmail, string toName, string productTitle, decimal newBidAmount)
    {
        var subject = $"You have been outbid on \"{productTitle}\"";
        var body = $@"
            <h2>You've been outbid!</h2>
            <p>Hello {toName},</p>
            <p>Someone has placed a higher bid of <strong>?{newBidAmount:N2}</strong> on <strong>{productTitle}</strong>.</p>
            <p>Visit BidSnap to place a higher bid and stay in the game!</p>
            <br/>
            <p>Regards,<br/>BidSnap Team</p>";

        await SendEmailAsync(toEmail, toName, subject, body);
    }

    public async Task SendAuctionWonNotificationAsync(string toEmail, string toName, string productTitle, decimal winningAmount)
    {
        var subject = $"Congratulations! You won the auction for \"{productTitle}\"";
        var body = $@"
            <h2>?? You Won!</h2>
            <p>Hello {toName},</p>
            <p>Congratulations! You have won the auction for <strong>{productTitle}</strong> with a winning bid of <strong>?{winningAmount:N2}</strong>.</p>
            <p>The seller will contact you shortly to complete the transaction.</p>
            <br/>
            <p>Regards,<br/>BidSnap Team</p>";

        await SendEmailAsync(toEmail, toName, subject, body);
    }

    public async Task SendSuspensionNotificationAsync(string toEmail, string toName, string reason, DateTime suspensionEndDate)
    {
        var subject = "Your BidSnap account has been suspended";
        var body = $@"
            <h2>Account Suspended</h2>
            <p>Hello {toName},</p>
            <p>Your account has been suspended for the following reason:</p>
            <p><em>{reason}</em></p>
            <p>Suspension will be lifted on: <strong>{suspensionEndDate:MMMM dd, yyyy}</strong></p>
            <p>You may pay a penalty fee to lift the suspension early.</p>
            <br/>
            <p>Regards,<br/>BidSnap Team</p>";

        await SendEmailAsync(toEmail, toName, subject, body);
    }

    public async Task SendMembershipActivatedNotificationAsync(string toEmail, string toName, string membershipName, DateTime expiryDate)
    {
        var subject = $"Your {membershipName} membership is now active!";
        var body = $@"
            <h2>Membership Activated</h2>
            <p>Hello {toName},</p>
            <p>Your <strong>{membershipName}</strong> membership has been successfully activated!</p>
            <p>Valid until: <strong>{expiryDate:MMMM dd, yyyy}</strong></p>
            <p>Enjoy your enhanced features on BidSnap!</p>
            <br/>
            <p>Regards,<br/>BidSnap Team</p>";

        await SendEmailAsync(toEmail, toName, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        try
        {
            var smtpSettings = _config.GetSection("SmtpSettings");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtpSettings["SenderName"], smtpSettings["SenderEmail"]));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                smtpSettings["Server"],
                int.Parse(smtpSettings["Port"]!),
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(smtpSettings["Username"], smtpSettings["Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
        }
    }
}
