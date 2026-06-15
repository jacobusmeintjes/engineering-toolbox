using Messaging.Abstractions;

namespace Messaging.Events.Payments
{
    // Events/Payments/PaymentFailed.cs
    public record PaymentFailed : EventBase
    {
        public override string EventType => "oms.payments.failed";

        public required Guid OrderId { get; init; }
        public required Guid CustomerId { get; init; }
        public required string FailureReason { get; init; }
    }
}
