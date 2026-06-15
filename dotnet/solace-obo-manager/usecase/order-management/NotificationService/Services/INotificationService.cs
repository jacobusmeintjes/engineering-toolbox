using Contracts.Requests;

namespace NotificationService.Services
{
    // Services/INotificationService.cs
    public interface INotificationService
    {
        Task SendAsync(SendNotificationRequest request, CancellationToken ct);
    }
}
