using Contracts.Domain;
using Contracts.Requests;
using Contracts.Responses;
using OrderService.Domain;
using OrderService.Exceptions;
using OrderService.HttpClients;
using OrderService.Repositories;

namespace OrderService.Services
{
    // Oms.OrderService/Services/OrderOrchestrator.cs
    public class RestOrchestrator(
        IOrderRepository orders,
        IPaymentClient payment,
        IInventoryClient inventory,
        IFulfilmentClient fulfilment,
        INotificationClient notification) : IOrderOrchestrator
    {
        public async Task<OrderResponse> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken ct)
        {
            // 1. Persist in Draft — gives us an ID before any downstream calls
            var order = Order.Create(request.CustomerId, request.ShippingAddress, request.Items);
            await orders.SaveAsync(order, ct);

            // 2. Authorise payment — blocks here
            order.Transition(OrderStatus.PendingPayment);
            await orders.UpdateAsync(order, ct);

            var paymentResult = await payment.AuthoriseAsync(new AuthorisePaymentRequest(
                order.Id,
                order.TotalAmount,
                request.PaymentMethodToken), ct);

            if (!paymentResult.Success)
            {
                order.Transition(OrderStatus.CancelledPaymentFailed);
                await orders.UpdateAsync(order, ct);
                throw new PaymentFailedException(paymentResult.FailureReason);
            }

#pragma warning disable CS8604 // Possible null reference argument.
            order.SetPaymentTransaction(paymentResult.TransactionId);
#pragma warning restore CS8604 // Possible null reference argument.
            
            order.Transition(OrderStatus.PaymentAuthorised);
            await orders.UpdateAsync(order, ct);

            // 3. Reserve inventory — blocks here
            var reserveResult = await inventory.ReserveAsync(new ReserveStockRequest(
                order.Id,
                order.LineItems.Select(i => new StockReservation(i.Sku, i.Quantity)).ToList()), ct);

            if (!reserveResult.Success)
            {
                // Compensate: release the payment authorisation
                await payment.VoidAsync(new VoidPaymentRequest(paymentResult.TransactionId), ct);
                order.Transition(OrderStatus.CancelledNoStock);
                await orders.UpdateAsync(order, ct);
                throw new InsufficientStockException(reserveResult.OutOfStockSkus);
            }

            order.Transition(OrderStatus.Confirmed);
            await orders.UpdateAsync(order, ct);

            // 4. Create shipment — blocks here
            var shipmentResult = await fulfilment.CreateShipmentAsync(new CreateShipmentRequest(
                order.Id,
                order.CustomerId,
                order.ShippingAddress,
                order.LineItems.Select(item => 
                new OrderItem(item.ProductId, 
                                item.Sku, 
                                item.ProductName,
                                item.Quantity, 
                                item.UnitPrice))
                                    .ToList().AsReadOnly()),
                ct);

            order.SetShipment(shipmentResult.ShipmentId);
            order.Transition(OrderStatus.Picking);
            await orders.UpdateAsync(order, ct);

            // 5. Send confirmation — still blocking the client response
            await notification.SendAsync(new SendNotificationRequest(
                order.CustomerId,
                "OrderConfirmed",
                order.Id,
                order.TotalAmount), ct);

            return order.ToResponse();
        }
    }
}
