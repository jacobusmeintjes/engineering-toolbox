using Messaging.Abstractions;

namespace Messaging.Events.Fulfilment
{
    // Events/Fulfilment/ShipmentPacked.cs
    public record ShipmentPacked : EventBase
    {
        public override string EventType => "oms.fulfilment.shipment-packed";

        public required Guid ShipmentId { get; init; }
        public required Guid OrderId { get; init; }
    }
}
