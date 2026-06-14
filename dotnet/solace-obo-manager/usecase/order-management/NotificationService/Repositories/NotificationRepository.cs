using Microsoft.EntityFrameworkCore;
using NotificationService.Domain;

namespace NotificationService.Repositories
{
    // Repositories/NotificationRepository.cs
    public class NotificationRepository(NotificationDbContext db) : INotificationRepository
    {
        public async Task SaveAsync(NotificationRecord record, CancellationToken ct)
        {
            await db.Notifications.AddAsync(record, ct);
            await db.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<NotificationRecord>> GetByOrderIdAsync(
            Guid orderId, CancellationToken ct) =>
            await db.Notifications
                .Where(n => n.OrderId == orderId)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<NotificationRecord>> GetByCustomerIdAsync(
            Guid customerId, CancellationToken ct) =>
            await db.Notifications
                .Where(n => n.CustomerId == customerId)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync(ct);
    }
}
