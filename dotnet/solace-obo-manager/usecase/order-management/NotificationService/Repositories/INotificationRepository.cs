using NotificationService.Domain;
using System.Reflection.Emit;

namespace NotificationService.Repositories
{
    // Repositories/INotificationRepository.cs
    public interface INotificationRepository
    {
        Task SaveAsync(NotificationRecord record, CancellationToken ct);
        Task<IReadOnlyList<NotificationRecord>> GetByOrderIdAsync(
            Guid orderId, CancellationToken ct);
        Task<IReadOnlyList<NotificationRecord>> GetByCustomerIdAsync(
            Guid customerId, CancellationToken ct);
    }
}
