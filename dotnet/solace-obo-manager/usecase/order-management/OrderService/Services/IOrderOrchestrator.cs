using Contracts.Requests;
using Contracts.Responses;

namespace OrderService.Services
{
    // Services/IOrderOrchestrator.cs
    public interface IOrderOrchestrator
    {
        Task<OrderResponse> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken ct);
    }
}
