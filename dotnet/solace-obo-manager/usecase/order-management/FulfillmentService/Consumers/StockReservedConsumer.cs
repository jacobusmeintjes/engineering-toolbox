using Contracts.Requests;
using FulfillmentService.Domain;
using FulfillmentService.Repositories;
using FulfillmentService.Services;
using Messaging.Abstractions;
using Messaging.Events.Fulfilment;
using Messaging.Events.Inventory;
using Messaging.Topics;
using OrderService.HttpClients;

namespace FulfilmentService.Consumers
{
    // Consumers/StockReservedConsumer.cs
    public class StockReservedConsumer(
        IShipmentRepository repo,
        IWarehouseSystem warehouse,
        IEventPublisher publisher,
        ILogger<StockReservedConsumer> logger) : IEventHandler<StockReserved>
    {
        public async Task HandleAsync(StockReserved @event, CancellationToken ct)
        {
            logger.LogInformation(
                "Processing StockReserved for order {OrderId}",
                @event.OrderId);

            // Idempotency guard
            var existing = await repo.GetByOrderIdAsync(@event.OrderId, ct);
            if (existing is not null)
            {
                logger.LogWarning(
                    "Shipment already created for order {OrderId} — skipping",
                    @event.OrderId);
                return;
            }

            // Create shipment aggregate
            var shipment = Shipment.Create(
                @event.OrderId,
                @event.CustomerId,
                @event.ShippingAddress,
                @event.Items);

            await repo.SaveAsync(shipment, ct);

            // Instruct warehouse to start picking
            await warehouse.CreatePickListAsync(
                shipment.Id, shipment.LineItems, ct);

            shipment.StartPicking();
            await repo.UpdateAsync(shipment, ct);

            // Publish ShipmentCreated — Order Service updates its status
            await publisher.PublishAsync(
                new ShipmentCreated
                {
                    ShipmentId = shipment.Id,
                    OrderId = @event.OrderId,
                    CustomerId = @event.CustomerId,
                    Amount = @event.Amount,
                    EstimatedDelivery = shipment.EstimatedDelivery
                },
                Topics.Fulfilment.ShipmentCreated, ct);

            logger.LogInformation(
                "Shipment {ShipmentId} created for order {OrderId} — notifying customer",
                shipment.Id, @event.OrderId);          
        }
    }
}
