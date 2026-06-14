using Contracts.Requests;
using Contracts.Responses;

namespace InventoryService.Services
{
    // Services/IInventoryService.cs
    public interface IInventoryService
    {
        Task<ReserveStockResponse> ReserveAsync(ReserveStockRequest request, CancellationToken ct);
        Task ReleaseAsync(ReleaseStockRequest request, CancellationToken ct);
    }
}
