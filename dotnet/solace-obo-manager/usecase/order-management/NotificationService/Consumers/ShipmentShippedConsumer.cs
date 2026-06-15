using Contracts.Requests;
using Messaging.Abstractions;
using Messaging.Events.Fulfilment;
using NotificationService.Services;

namespace NotificationService.Consumers
{
    // Consumers/ShipmentShippedConsumer.cs
    public class ShipmentShippedConsumer(
        INotificationService notificationService,
        ILogger<ShipmentShippedConsumer> logger) : IEventHandler<ShipmentShipped>
    {
        public async Task HandleAsync(ShipmentShipped @event, CancellationToken ct)
        {
            logger.LogInformation(
                "Sending OrderShipped notification for order {OrderId}",
                @event.OrderId);

            await notificationService.SendAsync(
                new SendNotificationRequest(
                    @event.CustomerId,
                    "OrderShipped",
                    @event.OrderId,
                    null), ct);
        }
    }
}
