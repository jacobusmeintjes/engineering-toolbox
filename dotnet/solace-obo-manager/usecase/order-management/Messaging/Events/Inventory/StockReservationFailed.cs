using Messaging.Abstractions;

namespace Messaging.Events.Inventory
{
    // Events/Inventory/StockReservationFailed.cs
    public record StockReservationFailed : EventBase
    {
        public override string EventType => "oms.inventory.reservation-failed";

        public required Guid OrderId { get; init; }
        public required IReadOnlyList<string> OutOfStockSkus { get; init; }
    }
}
