using Messaging.Events.Fulfilment;
using Messaging.Infrastructure;
using Messaging.Topics;

namespace NotificationService.Consumers
{
    // Consumers/ShipmentShippedSubscriber.cs
    public class ShipmentShippedSubscriber(
        SolaceConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<ShipmentShippedSubscriber> logger)
        : SolaceSubscriber<ShipmentShipped>(
            connection,
            scopeFactory,
            logger,
            Topics.Fulfilment.ShipmentShipped)
    {
        protected override string QueueSuffix => "notification-service";
    }
}
