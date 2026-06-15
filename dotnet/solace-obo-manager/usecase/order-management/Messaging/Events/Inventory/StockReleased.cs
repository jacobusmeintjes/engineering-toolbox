using Contracts.Domain;
using Messaging.Abstractions;

namespace Messaging.Events.Inventory
{

    // Events/Inventory/StockReleased.cs
    public record StockReleased : EventBase
    {
        public override string EventType => "oms.inventory.released";

        public required Guid OrderId { get; init; }
        public required IReadOnlyList<StockReservation> Items { get; init; }
    }
}
