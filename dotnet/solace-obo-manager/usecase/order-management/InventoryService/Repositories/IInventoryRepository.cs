using InventoryService.Domain;

namespace InventoryService.Repositories
{
    // Repositories/IInventoryRepository.cs
    public interface IInventoryRepository
    {
        Task<StockItem?> GetBySkuAsync(string sku, CancellationToken ct);
        Task<IReadOnlyList<StockItem>> GetBySkusAsync(
            IReadOnlyList<string> skus, CancellationToken ct);
        Task<IReadOnlyList<StockReservationRecord>> GetReservationsByOrderIdAsync(
            Guid orderId, CancellationToken ct);
        Task SaveStockItemAsync(StockItem item, CancellationToken ct);
        Task UpdateStockItemAsync(StockItem item, CancellationToken ct);
        Task SaveReservationAsync(StockReservationRecord reservation, CancellationToken ct);
        Task UpdateReservationAsync(StockReservationRecord reservation, CancellationToken ct);
    }
}
