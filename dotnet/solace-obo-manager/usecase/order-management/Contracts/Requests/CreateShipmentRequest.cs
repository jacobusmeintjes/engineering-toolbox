using Contracts.Domain;

namespace Contracts.Requests
{
    // Requests/CreateShipmentRequest.cs
    public record CreateShipmentRequest(
        Guid OrderId,
        string ShippingAddress,
        IReadOnlyList<OrderItem> Items);
}
