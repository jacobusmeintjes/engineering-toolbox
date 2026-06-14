namespace NotificationService.Services
{
    // Services/IEmailSender.cs
    public interface IEmailSender
    {
        Task SendAsync(
            string toAddress,
            string subject,
            string body,
            CancellationToken ct);
    }
}
