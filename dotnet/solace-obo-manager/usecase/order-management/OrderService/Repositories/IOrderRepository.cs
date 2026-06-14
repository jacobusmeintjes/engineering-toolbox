using Microsoft.EntityFrameworkCore;
using OrderService.Domain;

namespace OrderService.Repositories
{
    public interface IOrderRepository
    {
        Task SaveAsync(Order order, CancellationToken ct);
        Task UpdateAsync(Order order, CancellationToken ct);
    }

    // Repositories/OrderRepository.cs
    public class OrderRepository(OrderDbContext db) : IOrderRepository
    {
        public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct) =>
            await db.Orders
                .Include(o => o.LineItems)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct);

        public async Task SaveAsync(Order order, CancellationToken ct)
        {
            await db.Orders.AddAsync(order, ct);
            await db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Order order, CancellationToken ct)
        {
            db.Orders.Update(order);
            await db.SaveChangesAsync(ct);
        }
    }
}
