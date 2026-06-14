namespace PaymentService.Domain
{
    public class PaymentRecord
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public decimal Amount { get; private set; }
        public PaymentStatus Status { get; private set; }
        public string? TransactionId { get; private set; }
        public string? FailureReason { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }

        private PaymentRecord() { }  // EF

        public static PaymentRecord CreateAuthorised(
            Guid orderId, decimal amount, string transactionId) => new()
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Amount = amount,
                Status = PaymentStatus.Authorised,
                TransactionId = transactionId,
                CreatedAt = DateTimeOffset.UtcNow
            };

        public static PaymentRecord CreateFailed(
            Guid orderId, decimal amount, string failureReason) => new()
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Amount = amount,
                Status = PaymentStatus.Failed,
                FailureReason = failureReason,
                CreatedAt = DateTimeOffset.UtcNow
            };

        public void Void()
        {
            if (Status != PaymentStatus.Authorised)
                throw new InvalidOperationException(
                    $"Cannot void a payment in status {Status}");

            Status = PaymentStatus.Voided;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    // Domain/PaymentStatus.cs
    public enum PaymentStatus
    {
        Authorised,
        Captured,
        Voided,
        Refunded,
        Failed
    }
}
