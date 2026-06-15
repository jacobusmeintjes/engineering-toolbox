using Contracts.Responses;

namespace NotificationService.Domain
{
    // Domain/NotificationRecord.cs
    public class NotificationRecord
    {
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public string EventType { get; private set; } = default!;
        public Guid OrderId { get; private set; }
        public NotificationChannel Channel { get; private set; }
        public NotificationStatus Status { get; private set; }
        public string? FailureReason { get; private set; }
        public DateTimeOffset SentAt { get; private set; }

        private NotificationRecord() { }  // EF

        public static NotificationRecord CreateSuccess(
            Guid customerId,
            Guid orderId,
            string eventType,
            NotificationChannel channel) => new()
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                OrderId = orderId,
                EventType = eventType,
                Channel = channel,
                Status = NotificationStatus.Sent,
                SentAt = DateTimeOffset.UtcNow
            };

        public static NotificationRecord CreateFailed(
            Guid customerId,
            Guid orderId,
            string eventType,
            NotificationChannel channel,
            string failureReason) => new()
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                OrderId = orderId,
                EventType = eventType,
                Channel = channel,
                Status = NotificationStatus.Failed,
                FailureReason = failureReason,
                SentAt = DateTimeOffset.UtcNow
            };

        // Domain/NotificationRecord.cs — add mapping method
        public NotificationLogResponse ToResponse() => new(
            Id,
            CustomerId,
            OrderId,
            EventType,
            Channel.ToString(),
            Status.ToString(),
            FailureReason,
            SentAt);
    }
}
