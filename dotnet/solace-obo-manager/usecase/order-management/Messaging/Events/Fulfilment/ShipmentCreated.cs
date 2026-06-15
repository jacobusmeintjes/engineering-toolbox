using Messaging.Abstractions;

namespace Messaging.Events.Fulfilment
{
    // Events/Fulfilment/ShipmentCreated.cs
    public record ShipmentCreated : EventBase
    {
        public override string EventType => "oms.fulfilment.shipment-created";

        public required Guid ShipmentId { get; init; }
        public required Guid OrderId { get; init; }
        public required Guid CustomerId { get; init; }
        public required decimal Amount { get; init; }           // ← carried forward

        public required DateTimeOffset EstimatedDelivery { get; init; }
    }
}
