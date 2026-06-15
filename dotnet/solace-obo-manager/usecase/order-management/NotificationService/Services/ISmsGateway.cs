namespace NotificationService.Services
{
    // Services/ISmsGateway.cs
    public interface ISmsGateway
    {
        Task SendAsync(
            string toNumber,
            string message,
            CancellationToken ct);
    }
}
