using Messaging.Events.Payments;
using Messaging.Infrastructure;
using Messaging.Topics;

namespace InventoryService.Consumer
{
    public class PaymentFailedSubscriber(
        SolaceConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentFailedSubscriber> logger)
        : SolaceSubscriber<PaymentFailed>(
            connection,
            scopeFactory,
            logger,
            Topics.Payments.Failed)
    {
        protected override string QueueSuffix => "inventory-service";
    }
}
