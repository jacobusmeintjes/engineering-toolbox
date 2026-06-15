using Messaging.Events.Orders;
using Messaging.Infrastructure;
using Messaging.Topics;

namespace PaymentService.Consumers
{
    // Consumers/OrderPlacedSubscriber.cs
    public class OrderPlacedSubscriber(
        SolaceConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<OrderPlacedSubscriber> logger)
        : SolaceSubscriber<OrderPlaced>(
            connection,
            scopeFactory,
            logger,
            Topics.Orders.Placed)
    {
        protected override string QueueSuffix => "payment-service";
    }
}
