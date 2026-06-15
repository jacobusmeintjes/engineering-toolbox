using Contracts.Domain;

namespace Contracts.Requests
{
    // Requests/ReleaseStockRequest.cs
    public record ReleaseStockRequest(
        Guid OrderId,
        IReadOnlyList<StockReservation> Items);
}
