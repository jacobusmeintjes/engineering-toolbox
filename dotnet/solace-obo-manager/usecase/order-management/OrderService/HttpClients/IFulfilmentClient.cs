using Contracts.Requests;
using Contracts.Responses;

namespace OrderService.HttpClients
{
    // HttpClients/IFulfilmentClient.cs
    public interface IFulfilmentClient
    {
        Task<CreateShipmentResponse> CreateShipmentAsync(
            CreateShipmentRequest request, CancellationToken ct);
    }

    // HttpClients/FulfilmentClient.cs
    public class FulfilmentClient(HttpClient http) : IFulfilmentClient
    {
        public async Task<CreateShipmentResponse> CreateShipmentAsync(
            CreateShipmentRequest request, CancellationToken ct)
        {
            var response = await http.PostAsJsonAsync("fulfilment/shipments", request, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CreateShipmentResponse>(ct)
                ?? throw new InvalidOperationException("Empty response from Fulfilment Service");
        }
    }
}
