using Contracts.Domain;
using Contracts.Requests;
using InventoryService.Repositories;
using InventoryService.Services;
using Messaging.Abstractions;
using Messaging.Events.Inventory;
using Messaging.Events.Payments;
using Messaging.Topics;
using OrderService.HttpClients;

namespace InventoryService.Consumer
{
    public class PaymentAuthorisedConsumer(
        IInventoryService inventoryService,
        IInventoryRepository inventoryRepository,
        IEventPublisher publisher,
        ILogger<PaymentAuthorisedConsumer> logger) : IEventHandler<PaymentAuthorised>
    {

        public async Task HandleAsync(PaymentAuthorised @event, CancellationToken ct)
        {
            logger.LogInformation(
        "Processing PaymentAuthorised for order {OrderId}",
        @event.OrderId);

            // Idempotency — check if we already reserved for this order
            var existing = await inventoryRepository.GetReservationsByOrderIdAsync(@event.OrderId, ct);
            if (existing.Any())
            {
                logger.LogWarning(
                    "Stock already reserved for order {OrderId} — skipping",
                    @event.OrderId);
                return;
            }

            // We need the items — they came from OrderPlaced which Payment forwarded
            // via the PaymentAuthorised event payload
            var reserveRequest = new ReserveStockRequest(
                @event.OrderId,
                @event.Items
                    .Select(i => new StockReservation(i.Sku, i.Quantity))
                    .ToList());

            var reserveResult = await inventoryService.ReserveAsync(reserveRequest, ct);

            if (!reserveResult.Success)
            {
                logger.LogWarning(
                    "Stock reservation failed for order {OrderId} — out of stock: {Skus}",
                    @event.OrderId,
                    string.Join(", ", reserveResult.OutOfStockSkus));

                // Publish — Payment Service will react and void
                await publisher.PublishAsync(
                    new StockReservationFailed
                    {
                        OrderId = @event.OrderId,
                        OutOfStockSkus = reserveResult.OutOfStockSkus
                    },
                    Topics.Inventory.ReservationFailed, ct);

                return;
            }

            // Publish success
            await publisher.PublishAsync(
                new StockReserved
                {
                    OrderId = @event.OrderId,
                    Items = @event.Items,
                    CustomerId = @event.CustomerId,
                    Amount = @event.Amount,
                    ShippingAddress = @event.ShippingAddress,
                    EventId = @event.EventId,
                    OccurredAt = @event.OccurredAt
                },
                Topics.Inventory.Reserved, ct);

            logger.LogInformation(
                "Stock reserved for order {OrderId} — continuing chain via HTTP",
                @event.OrderId);
        }    
    }


    public class PaymentFailedConsumer : IEventHandler<PaymentFailed>
    {
        public Task HandleAsync(PaymentFailed @event, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
