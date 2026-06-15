using Contracts.Requests;
using Messaging.Abstractions;
using Messaging.Events.Fulfilment;
using NotificationService.Services;

namespace NotificationService.Consumers
{
    // Consumers/ShipmentCreatedConsumer.cs
    public class ShipmentCreatedConsumer(
        INotificationService notificationService,
        ILogger<ShipmentCreatedConsumer> logger) : IEventHandler<ShipmentCreated>
    {
        public async Task HandleAsync(ShipmentCreated @event, CancellationToken ct)
        {
            logger.LogInformation(
                "Sending OrderConfirmed notification for order {OrderId}",
                @event.OrderId);

            await notificationService.SendAsync(
                new SendNotificationRequest(
                    @event.CustomerId,
                    "OrderConfirmed",
                    @event.OrderId,
                    @event.Amount), ct);
        }
    }
}
