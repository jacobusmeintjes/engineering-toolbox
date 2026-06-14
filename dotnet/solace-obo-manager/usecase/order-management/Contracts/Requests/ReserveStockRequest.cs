using Contracts.Domain;

namespace Contracts.Requests
{
    // Requests/ReserveStockRequest.cs
    public record ReserveStockRequest(
        Guid OrderId,
        IReadOnlyList<StockReservation> Items);
}
