namespace Contracts.Responses
{
    // Oms.Contracts/Responses/ShipmentResponse.cs
    public record ShipmentResponse(
        Guid ShipmentId,
        Guid OrderId,
        string Status,
        string ShippingAddress,
        string? TrackingNumber,
        string? CarrierCode,
        DateTimeOffset EstimatedDelivery,
        DateTimeOffset? ShippedAt,
        DateTimeOffset? DeliveredAt);


}
