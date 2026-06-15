using Contracts.Domain;
using Contracts.Requests;
using Messaging.Abstractions;
using Messaging.Events.Orders;
using Messaging.Events.Payments;
using Messaging.Topics;
using OrderService.HttpClients;
using PaymentService.Domain;
using PaymentService.Repositories;
using PaymentService.Services;

namespace PaymentService.Consumers
{
    // Consumers/OrderPlacedConsumer.cs
    public class OrderPlacedConsumer(
        IPaymentGateway gateway,
        IPaymentRepository repo,
        IEventPublisher publisher,
        IInventoryClient inventory,
        IFulfilmentClient fulfilment,
        INotificationClient notification,
        ILogger<OrderPlacedConsumer> logger) : IEventHandler<OrderPlaced>
    {
        public async Task HandleAsync(OrderPlaced @event, CancellationToken ct)
        {
            logger.LogInformation(
                "Processing OrderPlaced for order {OrderId} — amount {Amount}",
                @event.OrderId, @event.TotalAmount);

            // ── Idempotency ──────────────────────────────────────────────────────
            var existing = await repo.GetByOrderIdAsync(@event.OrderId, ct);
            if (existing is not null)
            {
                logger.LogWarning(
                    "Payment already processed for order {OrderId} — skipping",
                    @event.OrderId);
                return;
            }

            // ── Step 1: Authorise payment ─────────────────────────────────────
            var result = await gateway.AuthoriseAsync(
                @event.PaymentMethodToken, @event.TotalAmount, ct);

            if (!result.Success)
            {
                var failedRecord = PaymentRecord.CreateFailed(
                    @event.OrderId, @event.TotalAmount, result.FailureReason!);

                await repo.SaveAsync(failedRecord, ct);

                await publisher.PublishAsync(
                    new PaymentFailed
                    {
                        OrderId = @event.OrderId,
                        CustomerId = @event.CustomerId,
                        FailureReason = result.FailureReason!
                    },
                    Topics.Payments.Failed, ct);

                logger.LogWarning(
                    "Payment failed for order {OrderId} — reason: {Reason}",
                    @event.OrderId, result.FailureReason);

                return;
            }

            var record = PaymentRecord.CreateAuthorised(
                @event.OrderId, @event.TotalAmount, result.TransactionId!);

            await repo.SaveAsync(record, ct);

            await publisher.PublishAsync(
                new PaymentAuthorised
                {
                    OrderId = @event.OrderId,
                    CustomerId = @event.CustomerId,
                    TransactionId = result.TransactionId!,
                    Amount = @event.TotalAmount
                },
                Topics.Payments.Authorised, ct);

            logger.LogInformation(
                "Payment authorised for order {OrderId} — transaction {TransactionId}",
                @event.OrderId, result.TransactionId);

            // ── Step 2: Reserve inventory via HTTP ────────────────────────────
            var reserveRequest = new ReserveStockRequest(
                @event.OrderId,
                @event.Items
                    .Select(i => new StockReservation(i.Sku, i.Quantity))
                    .ToList());

            var reserveResult = await inventory.ReserveAsync(reserveRequest, ct);

            if (!reserveResult.Success)
            {
                logger.LogWarning(
                    "Stock reservation failed for order {OrderId} — out of stock: {Skus}",
                    @event.OrderId,
                    string.Join(", ", reserveResult.OutOfStockSkus));

                // Compensate — void the authorised payment
                await gateway.VoidAsync(result.TransactionId!, ct);
                record.Void();
                await repo.UpdateAsync(record, ct);

                await publisher.PublishAsync(
                    new PaymentVoided
                    {
                        OrderId = @event.OrderId,
                        TransactionId = result.TransactionId!
                    },
                    Topics.Payments.Voided, ct);

                logger.LogWarning(
                    "Payment voided for order {OrderId} after stock failure",
                    @event.OrderId);

                return;
            }

            logger.LogInformation(
                "Stock reserved for order {OrderId} — passing to Fulfilment",
                @event.OrderId);

            // ── Step 3: Create shipment via HTTP ──────────────────────────────
            var shipmentResult = await fulfilment.CreateShipmentAsync(
                new CreateShipmentRequest(
                    @event.OrderId,
                    @event.ShippingAddress,
                    @event.Items), ct);

            logger.LogInformation(
                "Shipment {ShipmentId} created for order {OrderId}",
                shipmentResult.ShipmentId, @event.OrderId);

            // ── Step 4: Send confirmation via HTTP ────────────────────────────
            await notification.SendAsync(
                new SendNotificationRequest(
                    @event.CustomerId,
                    "OrderConfirmed",
                    @event.OrderId,
                    @event.TotalAmount), ct);

            logger.LogInformation(
                "Order {OrderId} fully processed — stage 1 complete",
                @event.OrderId);
        }
    }
}
