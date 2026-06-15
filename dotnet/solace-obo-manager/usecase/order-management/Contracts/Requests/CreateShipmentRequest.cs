using Contracts.Domain;

namespace Contracts.Requests
{
    // Requests/CreateShipmentRequest.cs
    public record CreateShipmentRequest(
        Guid OrderId,
        Guid CustomerId,
        string ShippingAddress,
        IReadOnlyList<OrderItem> Items);
}
