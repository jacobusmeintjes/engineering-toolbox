using Contracts.Domain;
using Messaging.Abstractions;

namespace Messaging.Events.Inventory
{
    // Events/Inventory/StockReserved.cs
    public record StockReserved : EventBase
    {
        public override string EventType => "oms.inventory.reserved";

        public required Guid OrderId { get; init; }
        public required IReadOnlyList<StockReservation> Items { get; init; }
    }
}
