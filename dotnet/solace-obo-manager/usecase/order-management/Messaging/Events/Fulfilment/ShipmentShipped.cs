using Messaging.Abstractions;

namespace Messaging.Events.Fulfilment
{
    // Events/Fulfilment/ShipmentShipped.cs
    public record ShipmentShipped : EventBase
    {
        public override string EventType => "oms.fulfilment.shipment-shipped";

        public required Guid ShipmentId { get; init; }
        public required Guid OrderId { get; init; }
        public required Guid CustomerId { get; init; }
        public required string TrackingNumber { get; init; }
        public required string CarrierCode { get; init; }
        public required DateTimeOffset EstimatedDelivery { get; init; }
    }
}
