using Contracts.Domain;
using System.Net.NetworkInformation;
using System.Threading.Channels;

namespace Contracts.Responses
{

    // Responses/OrderResponse.cs
    public record OrderResponse(
        Guid OrderId,
        OrderStatus Status,
        decimal TotalAmount,
        string ShippingAddress,
        IReadOnlyList<OrderItem> Items,
        string? ShipmentId,
        DateTimeOffset PlacedAt);

   

}
