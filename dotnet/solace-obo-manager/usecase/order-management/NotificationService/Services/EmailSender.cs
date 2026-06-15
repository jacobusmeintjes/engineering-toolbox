namespace NotificationService.Services
{
    // Services/EmailSender.cs  — stub, replace with SendGrid/Mailgun SDK
    public class EmailSender(ILogger<EmailSender> logger) : IEmailSender
    {
        public Task SendAsync(
            string toAddress,
            string subject,
            string body,
            CancellationToken ct)
        {
            // Replace with: SendGridClient.SendEmailAsync(...)
            logger.LogInformation(
                "Sending email to {ToAddress} — subject: {Subject}",
                toAddress, subject);

            return Task.CompletedTask;
        }
    }
}
