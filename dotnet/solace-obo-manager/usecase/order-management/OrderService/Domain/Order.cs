using Contracts.Domain;
using Contracts.Responses;

namespace OrderService.Domain
{
    // Domain/Order.cs
    public class Order
    {
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public OrderStatus Status { get; private set; }
        public string ShippingAddress { get; private set; } = default!;
        public decimal TotalAmount { get; private set; }
        public DateTimeOffset PlacedAt { get; private set; }
        public string? PaymentTransactionId { get; private set; }
        public string? ShipmentId { get; private set; }

        private readonly List<OrderLineItem> _lineItems = [];
        public IReadOnlyList<OrderLineItem> LineItems => _lineItems.AsReadOnly();

        private Order() { }  // EF

        public static Order Create(Guid customerId, string shippingAddress,
            IReadOnlyList<OrderItem> items)
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Status = OrderStatus.Draft,
                ShippingAddress = shippingAddress,
                TotalAmount = items.Sum(i => i.Quantity * i.UnitPrice),
                PlacedAt = DateTimeOffset.UtcNow
            };

            order._lineItems.AddRange(items.Select(i => OrderLineItem.From(order.Id, i)));
            return order;
        }

        public void Transition(OrderStatus next)
        {
            var allowed = Status switch
            {
                OrderStatus.Draft => new[] { OrderStatus.PendingPayment },
                OrderStatus.PendingPayment => new[] { OrderStatus.PaymentAuthorised,
                                                     OrderStatus.CancelledPaymentFailed },
                OrderStatus.PaymentAuthorised => new[] { OrderStatus.Confirmed,
                                                     OrderStatus.CancelledNoStock },
                OrderStatus.Confirmed => new[] { OrderStatus.Picking },
                OrderStatus.Picking => new[] { OrderStatus.Shipped },
                OrderStatus.Shipped => new[] { OrderStatus.Delivered },
                _ => Array.Empty<OrderStatus>()
            };

            if (!allowed.Contains(next))
                throw new InvalidOperationException(
                    $"Cannot transition from {Status} to {next}");

            Status = next;
        }

        public void SetPaymentTransaction(string transactionId) =>
            PaymentTransactionId = transactionId;

        public void SetShipment(string shipmentId) =>
            ShipmentId = shipmentId;

        public OrderResponse ToResponse() => new(
            Id,
            Status,
            TotalAmount,
            ShippingAddress,
            _lineItems.Select(li => li.ToContractItem()).ToList(),
            ShipmentId,
            PlacedAt);
    }
}
