using Messaging.Abstractions;

namespace Messaging.Events.Orders
{
    // Events/Orders/OrderConfirmed.cs
    public record OrderConfirmed : EventBase
    {
        public override string EventType => "oms.orders.confirmed";

        public required Guid OrderId { get; init; }
        public required Guid CustomerId { get; init; }
        public required decimal TotalAmount { get; init; }
        public required string PaymentTransactionId { get; init; }
        public required string ShipmentId { get; init; }
    }
}
