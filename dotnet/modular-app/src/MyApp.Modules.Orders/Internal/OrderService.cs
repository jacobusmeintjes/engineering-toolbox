using MyApp.Modules.Orders.Internal.Models;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Modules.Orders.Internal
{
    public interface IOrderService
    {
        Task<Guid> AddOrderAsync(Order order, CancellationToken ct);
        Task CancelOrderAsync(Guid orderId, CancellationToken ct);
        Task<Order?> GetOrderAsync(Guid orderId, CancellationToken ct);
    }

    public class OrderService : IOrderService
    {
        public ConcurrentDictionary<Guid, Order> Orders = new ConcurrentDictionary<Guid, Order>();

        public async Task<Guid> AddOrderAsync(Order order, CancellationToken ct)
        {
            if (order.Items.Count == 0)
                throw new ValidationException("Order must have at least one item");

            Orders.TryAdd(order.Id, order);
            return order.Id;
        }

        public async Task CancelOrderAsync(Guid orderId, CancellationToken ct)
        {
            var order = await GetOrderAsync(orderId, ct);
            order?.UpdateStatus(OrderStatus.Cancelled);
        }

        public async Task<Order?> GetOrderAsync(Guid orderId, CancellationToken ct)
        {
            Orders.TryGetValue(orderId, out var order);

            if (order is null)
            {
                throw new Exception("Order not found");
            }

            return order;
        }
    }
}
