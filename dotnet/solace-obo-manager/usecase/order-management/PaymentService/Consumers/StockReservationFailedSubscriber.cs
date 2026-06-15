using Messaging.Events.Inventory;
using Messaging.Infrastructure;
using Messaging.Topics;

namespace PaymentService.Consumers
{
    // Consumers/StockReservationFailedSubscriber.cs
    public class StockReservationFailedSubscriber(
        SolaceConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<StockReservationFailedSubscriber> logger)
        : SolaceSubscriber<StockReservationFailed>(
            connection,
            scopeFactory,
            logger,
            Topics.Inventory.ReservationFailed)
    {
        protected override string QueueSuffix => "payment-service";
    }
}
