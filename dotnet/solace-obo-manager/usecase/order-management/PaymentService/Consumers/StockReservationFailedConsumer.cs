using Messaging.Abstractions;
using Messaging.Events.Inventory;
using Messaging.Events.Payments;
using Messaging.Topics;
using PaymentService.Domain;
using PaymentService.Repositories;
using PaymentService.Services;

namespace PaymentService.Consumers
{
    // Consumers/StockReservationFailedConsumer.cs
    public class StockReservationFailedConsumer(
        IPaymentGateway gateway,
        IPaymentRepository repo,
        IEventPublisher publisher,
        ILogger<StockReservationFailedConsumer> logger) : IEventHandler<StockReservationFailed>
    {
        public async Task HandleAsync(StockReservationFailed @event, CancellationToken ct)
        {
            logger.LogInformation(
                "Stock reservation failed for order {OrderId} — voiding payment",
                @event.OrderId);

            var record = await repo.GetByOrderIdAsync(@event.OrderId, ct);

            if (record is null)
            {
                logger.LogWarning(
                    "No payment record found for order {OrderId} — cannot void",
                    @event.OrderId);
                return;
            }

            if (record.Status != PaymentStatus.Authorised)
            {
                logger.LogWarning(
                    "Payment for order {OrderId} is {Status} — skipping void",
                    @event.OrderId, record.Status);
                return;
            }

            await gateway.VoidAsync(record.TransactionId!, ct);
            record.Void();
            await repo.UpdateAsync(record, ct);

            await publisher.PublishAsync(
                new PaymentVoided
                {
                    OrderId = @event.OrderId,
                    TransactionId = record.TransactionId!
                },
                Topics.Payments.Voided, ct);

            logger.LogInformation(
                "Payment voided for order {OrderId} — transaction {TransactionId}",
                @event.OrderId, record.TransactionId);
        }
    }
}
