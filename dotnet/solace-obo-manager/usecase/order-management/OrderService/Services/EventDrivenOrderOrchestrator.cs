using Contracts.Requests;
using Contracts.Responses;
using Messaging.Abstractions;
using Messaging.Events.Orders;
using Messaging.Topics;
using OrderService.Domain;
using OrderService.Repositories;

namespace OrderService.Services
{
    // Services/EventDrivenOrderOrchestrator.cs
    public class EventDrivenOrderOrchestrator(
        IOrderRepository orders,
        IEventPublisher publisher,
        ILogger<EventDrivenOrderOrchestrator> logger) : IOrderOrchestrator
    {
        public async Task<OrderResponse> PlaceOrderAsync(
            PlaceOrderRequest request, CancellationToken ct)
        {
            // 1. Persist in Draft — same as REST path
            var order = Order.Create(
                request.CustomerId,
                request.ShippingAddress,
                request.Items);

            await orders.SaveAsync(order, ct);

            logger.LogInformation(
                "Order {OrderId} saved — publishing OrderPlaced event", order.Id);

            // 2. Publish event and return — no downstream HTTP calls
            var @event = new OrderPlaced
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                ShippingAddress = order.ShippingAddress,
                PaymentMethodToken = request.PaymentMethodToken,
                TotalAmount = order.TotalAmount,
                Items = request.Items
            };

            await publisher.PublishAsync(@event, Topics.Orders.Placed, ct);

            logger.LogInformation(
                "OrderPlaced event published for order {OrderId}", order.Id);

            // 3. Return 202 Accepted — processing continues asynchronously
            return order.ToResponse();
        }
    }
}
