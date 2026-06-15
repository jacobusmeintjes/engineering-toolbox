using Messaging.Events.Inventory;
using Messaging.Infrastructure;
using Messaging.Topics;

namespace PaymentService.Consumers
{
    // Consumers/PaymentVoidRequestedSubscriber.cs
    public class PaymentVoidRequestedSubscriber(
        SolaceConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentVoidRequestedSubscriber> logger)
        : SolaceSubscriber<StockReservationFailed>(
            connection,
            scopeFactory,
            logger,
            Topics.Inventory.ReservationFailed)
    {
        protected override string QueueSuffix => "payment-service";
    }
}
