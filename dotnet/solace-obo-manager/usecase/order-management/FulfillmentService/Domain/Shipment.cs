using Contracts.Domain;
using Contracts.Responses;

namespace FulfillmentService.Domain
{
    // Domain/Shipment.cs
    public class Shipment
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public string ShippingAddress { get; private set; } = default!;
        public ShipmentStatus Status { get; private set; }
        public string? TrackingNumber { get; private set; }
        public string? CarrierCode { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset EstimatedDelivery { get; private set; }
        public DateTimeOffset? ShippedAt { get; private set; }
        public DateTimeOffset? DeliveredAt { get; private set; }

        private readonly List<ShipmentLineItem> _lineItems = [];
        public IReadOnlyList<ShipmentLineItem> LineItems => _lineItems.AsReadOnly();

        private Shipment() { }  // EF

        public static Shipment Create(
            Guid orderId,
            string shippingAddress,
            IReadOnlyList<OrderItem> items)
        {
            var shipment = new Shipment
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ShippingAddress = shippingAddress,
                Status = ShipmentStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow,
                EstimatedDelivery = DateTimeOffset.UtcNow.AddBusinessDays(3)
            };

            shipment._lineItems.AddRange(
                items.Select(i => ShipmentLineItem.From(shipment.Id, i)));

            return shipment;
        }

        public void StartPicking()
        {
            Transition(ShipmentStatus.Picking);
        }

        public void MarkPacked()
        {
            Transition(ShipmentStatus.Packed);
        }

        public void MarkShipped(string trackingNumber, string carrierCode)
        {
            Transition(ShipmentStatus.Shipped);
            TrackingNumber = trackingNumber;
            CarrierCode = carrierCode;
            ShippedAt = DateTimeOffset.UtcNow;
        }

        public void MarkDelivered()
        {
            Transition(ShipmentStatus.Delivered);
            DeliveredAt = DateTimeOffset.UtcNow;
        }

        private void Transition(ShipmentStatus next)
        {
            var allowed = Status switch
            {
                ShipmentStatus.Pending => new[] { ShipmentStatus.Picking },
                ShipmentStatus.Picking => new[] { ShipmentStatus.Packed },
                ShipmentStatus.Packed => new[] { ShipmentStatus.Shipped },
                ShipmentStatus.Shipped => new[] { ShipmentStatus.Delivered },
                _ => Array.Empty<ShipmentStatus>()
            };

            if (!allowed.Contains(next))
                throw new InvalidOperationException(
                    $"Cannot transition shipment from {Status} to {next}");

            Status = next;
        }

        // Domain/Shipment.cs — add mapping method
        public ShipmentResponse ToResponse() => new(
            Id,
            OrderId,
            Status.ToString(),
            ShippingAddress,
            TrackingNumber,
            CarrierCode,
            EstimatedDelivery,
            ShippedAt,
            DeliveredAt);
    }
}
