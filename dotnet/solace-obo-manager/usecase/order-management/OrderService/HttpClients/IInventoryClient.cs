using Contracts.Requests;
using Contracts.Responses;

namespace OrderService.HttpClients
{
    // HttpClients/IInventoryClient.cs
    public interface IInventoryClient
    {
        Task<ReserveStockResponse> ReserveAsync(
            ReserveStockRequest request, CancellationToken ct);

        Task ReleaseAsync(ReleaseStockRequest request, CancellationToken ct);
    }

    // HttpClients/InventoryClient.cs
    public class InventoryClient(HttpClient http) : IInventoryClient
    {
        public async Task<ReserveStockResponse> ReserveAsync(
            ReserveStockRequest request, CancellationToken ct)
        {
            var response = await http.PutAsJsonAsync("inventory/reserve", request, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ReserveStockResponse>(ct)
                ?? throw new InvalidOperationException("Empty response from Inventory Service");
        }

        public async Task ReleaseAsync(ReleaseStockRequest request, CancellationToken ct)
        {
            var response = await http.PutAsJsonAsync("inventory/release", request, ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
