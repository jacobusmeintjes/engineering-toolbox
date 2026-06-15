using InventoryService.Domain;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Repositories
{
    // Repositories/InventoryRepository.cs
    public class InventoryRepository(InventoryDbContext db) : IInventoryRepository
    {
        public async Task<StockItem?> GetBySkuAsync(string sku, CancellationToken ct) =>
            await db.StockItems
                .FirstOrDefaultAsync(s => s.Sku == sku, ct);

        public async Task<IReadOnlyList<StockItem>> GetBySkusAsync(
            IReadOnlyList<string> skus, CancellationToken ct) =>
            await db.StockItems
                .Where(s => skus.Contains(s.Sku))
                .ToListAsync(ct);

        public async Task<IReadOnlyList<StockReservationRecord>> GetReservationsByOrderIdAsync(
            Guid orderId, CancellationToken ct) =>
            await db.Reservations
                .Where(r => r.OrderId == orderId)
                .ToListAsync(ct);

        public async Task SaveStockItemAsync(StockItem item, CancellationToken ct)
        {
            await db.StockItems.AddAsync(item, ct);
            await db.SaveChangesAsync(ct);
        }

        public async Task UpdateStockItemAsync(StockItem item, CancellationToken ct)
        {
            db.StockItems.Update(item);
            await db.SaveChangesAsync(ct);
        }

        public async Task SaveReservationAsync(StockReservationRecord reservation, CancellationToken ct)
        {
            await db.Reservations.AddAsync(reservation, ct);
            await db.SaveChangesAsync(ct);
        }

        public async Task UpdateReservationAsync(StockReservationRecord reservation, CancellationToken ct)
        {
            db.Reservations.Update(reservation);
            await db.SaveChangesAsync(ct);
        }
    }
}
