namespace Contracts.Responses
{
    // Responses/CreateShipmentResponse.cs
    public record CreateShipmentResponse(
        string ShipmentId,
        DateTimeOffset EstimatedDelivery);

}
