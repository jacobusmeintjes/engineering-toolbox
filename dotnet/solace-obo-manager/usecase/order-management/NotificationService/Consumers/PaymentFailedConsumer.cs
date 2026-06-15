using Contracts.Requests;
using Messaging.Abstractions;
using Messaging.Events.Payments;
using NotificationService.Services;

namespace NotificationService.Consumers
{
    // Consumers/PaymentFailedConsumer.cs
    public class PaymentFailedConsumer(
        INotificationService notificationService,
        ILogger<PaymentFailedConsumer> logger) : IEventHandler<PaymentFailed>
    {
        public async Task HandleAsync(PaymentFailed @event, CancellationToken ct)
        {
            logger.LogInformation(
                "Sending OrderCancelled notification for order {OrderId} — payment failed",
                @event.OrderId);

            await notificationService.SendAsync(
                new SendNotificationRequest(
                    @event.CustomerId,
                    "OrderCancelled",
                    @event.OrderId,
                    null), ct);
        }
    }
}
