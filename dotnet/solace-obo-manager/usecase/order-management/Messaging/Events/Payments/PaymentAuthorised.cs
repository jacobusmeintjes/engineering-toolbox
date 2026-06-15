using Messaging.Abstractions;

namespace Messaging.Events.Payments
{
    // Events/Payments/PaymentAuthorised.cs
    public record PaymentAuthorised : EventBase
    {
        public override string EventType => "oms.payments.authorised";

        public required Guid OrderId { get; init; }
        public required Guid CustomerId { get; init; }
        public required string TransactionId { get; init; }
        public required decimal Amount { get; init; }
    }
}
