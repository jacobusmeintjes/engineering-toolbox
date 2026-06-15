using Messaging.Events.Fulfilment;
using Messaging.Infrastructure;
using Messaging.Topics;

namespace NotificationService.Consumers
{
    // Consumers/ShipmentCreatedSubscriber.cs
    public class ShipmentCreatedSubscriber(
        SolaceConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<ShipmentCreatedSubscriber> logger)
        : SolaceSubscriber<ShipmentCreated>(
            connection,
            scopeFactory,
            logger,
            Topics.Fulfilment.ShipmentCreated)
    {
        protected override string QueueSuffix => "notification-service";
    }
}
