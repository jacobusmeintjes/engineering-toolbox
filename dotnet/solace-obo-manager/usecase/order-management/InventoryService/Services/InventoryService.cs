using Contracts.Requests;
using Contracts.Responses;
using InventoryService.Domain;
using InventoryService.Repositories;

namespace InventoryService.Services
{

    // Services/InventoryService.cs
    public class InventoryService(
        IInventoryRepository repo,
        ILogger<InventoryService> logger) : IInventoryService
    {
        public async Task<ReserveStockResponse> ReserveAsync(
            ReserveStockRequest request, CancellationToken ct)
        {
            var skus = request.Items.Select(i => i.Sku).ToList();
            var stockItems = await repo.GetBySkusAsync(skus, ct);

            // Check all SKUs exist and have sufficient stock before reserving any
            var outOfStock = new List<string>();

            foreach (var item in request.Items)
            {
                var stock = stockItems.FirstOrDefault(s => s.Sku == item.Sku);

                if (stock is null || stock.AvailableQuantity < item.Quantity)
                    outOfStock.Add(item.Sku);
            }

            if (outOfStock.Any())
            {
                logger.LogWarning(
                    "Reservation failed for order {OrderId} — out of stock: {Skus}",
                    request.OrderId, string.Join(", ", outOfStock));

                return new ReserveStockResponse(false, outOfStock);
            }

            // All available — reserve and record
            foreach (var item in request.Items)
            {
                var stock = stockItems.First(s => s.Sku == item.Sku);
                stock.Reserve(item.Quantity);
                await repo.UpdateStockItemAsync(stock, ct);

                var reservation = StockReservationRecord.Create(
                    request.OrderId, item.Sku, item.Quantity);
                await repo.SaveReservationAsync(reservation, ct);
            }

            logger.LogInformation(
                "Reserved stock for order {OrderId} across {Count} SKUs",
                request.OrderId, request.Items.Count);

            return new ReserveStockResponse(true, Array.Empty<string>());
        }

        public async Task ReleaseAsync(ReleaseStockRequest request, CancellationToken ct)
        {
            var reservations = await repo.GetReservationsByOrderIdAsync(request.OrderId, ct);
            var skus = reservations.Select(r => r.Sku).ToList();
            var stockItems = await repo.GetBySkusAsync(skus, ct);

            foreach (var reservation in reservations.Where(r =>
                r.Status == ReservationStatus.Active))
            {
                var stock = stockItems.First(s => s.Sku == reservation.Sku);
                stock.Release(reservation.Quantity);
                await repo.UpdateStockItemAsync(stock, ct);

                reservation.Release();
                await repo.UpdateReservationAsync(reservation, ct);
            }

            logger.LogInformation(
                "Released stock reservations for order {OrderId}", request.OrderId);
        }
    }
}
