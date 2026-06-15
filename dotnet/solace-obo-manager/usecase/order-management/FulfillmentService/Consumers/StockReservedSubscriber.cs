using Messaging.Events.Inventory;
using Messaging.Infrastructure;
using Messaging.Topics;

namespace FulfilmentService.Consumers
{
    // Consumers/StockReservedSubscriber.cs
    public class StockReservedSubscriber(
        SolaceConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<StockReservedSubscriber> logger)
        : SolaceSubscriber<StockReserved>(
            connection,
            scopeFactory,
            logger,
            Topics.Inventory.Reserved)
    {
        protected override string QueueSuffix => "fulfilment-service";
    }
}
