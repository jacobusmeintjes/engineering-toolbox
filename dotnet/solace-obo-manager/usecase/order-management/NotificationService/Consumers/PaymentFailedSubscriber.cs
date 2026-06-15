using Messaging.Events.Payments;
using Messaging.Infrastructure;
using Messaging.Topics;

namespace NotificationService.Consumers
{
    // Consumers/PaymentFailedSubscriber.cs
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
        protected override string QueueSuffix => "notification-service";
    }
}
