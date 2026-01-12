using MyApp.Modules.Orders.Public.Dtos;

namespace MyApp.Modules.Orders.Public
{
    public interface IOrdersModule
    {
        Task<OrderDto?> GetOrderAsync(Guid orderId, CancellationToken ct = default);
        Task<Guid> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct = default);
        Task CancelOrderAsync(Guid orderId, CancellationToken ct = default);
    }

    public class OrdersModule : IOrdersModule
    {
        private readonly Internal.IOrderService _orderService;

        public OrdersModule(Internal.IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<OrderDto?> GetOrderAsync(Guid orderId, CancellationToken ct = default)
        {
            // Implementation goes here
            Internal.Models.Order? order = await _orderService.GetOrderAsync(orderId, ct);

            if (order is null)
                return null;

            // Map internal model → public DTO
            return new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                TotalAmount = order.TotalAmount,
                Status = MapStatusToDto(order.Status),
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(item => new OrderLineItemDto
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList()
            };
        }
        public async Task<Guid> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct = default)
        {
            // Implementation goes here

            var order = Internal.Models.Order.Create(Guid.NewGuid(), request.CustomerId, Internal.Models.OrderStatus.Pending, DateTime.UtcNow);

            foreach (var item in request.Items)
            {
                order.AddLineItem(Internal.Models.OrderLineItem.Create(item.ProductId, item.Quantity, item.UnitPrice));
            }

            return await _orderService.AddOrderAsync(order, ct);
        }
        public async Task CancelOrderAsync(Guid orderId, CancellationToken ct = default)
        {
            await _orderService.CancelOrderAsync(orderId, ct);
        }


        // Helper method to map internal enum → public enum
        private static OrderStatusDto MapStatusToDto(Internal.Models.OrderStatus status)
        {
            return status switch
            {
                Internal.Models.OrderStatus.Pending => OrderStatusDto.Pending,
                Internal.Models.OrderStatus.Confirmed => OrderStatusDto.Confirmed,
                Internal.Models.OrderStatus.Shipped => OrderStatusDto.Shipped,
                Internal.Models.OrderStatus.Delivered => OrderStatusDto.Delivered,
                Internal.Models.OrderStatus.Cancelled => OrderStatusDto.Cancelled,
                _ => throw new ArgumentOutOfRangeException(nameof(status))
            };
        }
    }
}
