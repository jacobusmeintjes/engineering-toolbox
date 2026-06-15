using Contracts.Domain;
using Messaging.Abstractions;

namespace Messaging.Events.Inventory
{
    // Events/Inventory/StockReserved.cs
    public record StockReserved : EventBase
    {
        public override string EventType => "oms.inventory.reserved";

        public required Guid OrderId { get; init; }
        public required Guid CustomerId { get; init; }
        public required string ShippingAddress { get; init; }
        public required decimal Amount { get; init; }

        // Carried forward so Fulfilment has everything it needs
        public required IReadOnlyList<OrderItem> Items { get; init; }
    }
}
