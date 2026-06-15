using Messaging.Events.Payments;
using Messaging.Infrastructure;
using Messaging.Topics;

namespace InventoryService.Consumer
{
    public class PaymentAuthorisedSubscriber(
        SolaceConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentAuthorisedSubscriber> logger)
        : SolaceSubscriber<PaymentAuthorised>(
            connection,
            scopeFactory,
            logger,
            Topics.Payments.Authorised)
    {
        protected override string QueueSuffix => "inventory-service";
    }
}
