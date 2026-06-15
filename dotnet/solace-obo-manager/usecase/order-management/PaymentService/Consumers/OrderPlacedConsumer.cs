using Contracts.Domain;
using Contracts.Requests;
using Messaging.Abstractions;
using Messaging.Events.Orders;
using Messaging.Events.Payments;
using Messaging.Topics;
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
                    Amount = @event.TotalAmount,
                    Items = @event.Items,
                    ShippingAddress = @event.ShippingAddress,
                },
                Topics.Payments.Authorised, ct);

            logger.LogInformation(
                "Payment authorised for order {OrderId} — transaction {TransactionId}",
                @event.OrderId, result.TransactionId);
        }
    }
}
