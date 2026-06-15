using Messaging.Abstractions;

namespace Messaging.Events.Orders
{
    // Events/Orders/OrderCancelled.cs
    public record OrderCancelled : EventBase
    {
        public override string EventType => "oms.orders.cancelled";

        public required Guid OrderId { get; init; }
        public required Guid CustomerId { get; init; }
        public required string Reason { get; init; }
    }
}
