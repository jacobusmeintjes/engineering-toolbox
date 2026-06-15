using Contracts.Domain;
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

        // Carried forward so downstream services don't need to re-fetch
        public required string ShippingAddress { get; init; }
        public required IReadOnlyList<OrderItem> Items { get; init; }
    }
}
