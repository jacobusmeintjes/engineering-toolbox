using Contracts.Requests;
using Messaging.Abstractions;
using Messaging.Events.Fulfilment;
using NotificationService.Services;

namespace NotificationService.Consumers
{
    // Consumers/ShipmentDeliveredConsumer.cs
    public class ShipmentDeliveredConsumer(
        INotificationService notificationService,
        ILogger<ShipmentDeliveredConsumer> logger) : IEventHandler<ShipmentDelivered>
    {
        public async Task HandleAsync(ShipmentDelivered @event, CancellationToken ct)
        {
            logger.LogInformation(
                "Sending OrderDelivered notification for order {OrderId}",
                @event.OrderId);

            await notificationService.SendAsync(
                new SendNotificationRequest(
                    @event.CustomerId,
                    "OrderDelivered",
                    @event.OrderId,
                    null), ct);
        }
    }
}
