using Messaging.Events.Fulfilment;
using Messaging.Infrastructure;
using Messaging.Topics;

namespace NotificationService.Consumers
{
    // Consumers/ShipmentDeliveredSubscriber.cs
    public class ShipmentDeliveredSubscriber(
        SolaceConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<ShipmentDeliveredSubscriber> logger)
        : SolaceSubscriber<ShipmentDelivered>(
            connection,
            scopeFactory,
            logger,
            Topics.Fulfilment.ShipmentDelivered)
    {
        protected override string QueueSuffix => "notification-service";
    }
}
