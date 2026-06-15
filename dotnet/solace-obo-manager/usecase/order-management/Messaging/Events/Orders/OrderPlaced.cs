using Contracts.Domain;
using Messaging.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Events.Orders
{
    // Events/Orders/OrderPlaced.cs
    public record OrderPlaced : EventBase
    {
        public override string EventType => "oms.orders.placed";

        public required Guid OrderId { get; init; }
        public required Guid CustomerId { get; init; }
        public required string ShippingAddress { get; init; }
        public required string PaymentMethodToken { get; init; }
        public required decimal TotalAmount { get; init; }
        public required IReadOnlyList<OrderItem> Items { get; init; }
    }
}
