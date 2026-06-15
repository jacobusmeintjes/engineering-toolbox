using Messaging.Abstractions;

namespace Messaging.Events.Fulfilment
{
    // Events/Fulfilment/ShipmentDelivered.cs
    public record ShipmentDelivered : EventBase
    {
        public override string EventType => "oms.fulfilment.shipment-delivered";

        public required Guid ShipmentId { get; init; }
        public required Guid OrderId { get; init; }
        public required Guid CustomerId { get; init; }

        public required DateTimeOffset DeliveredAt { get; init; }
    }
}
