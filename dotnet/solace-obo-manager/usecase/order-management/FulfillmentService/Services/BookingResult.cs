namespace FulfillmentService.Services
{

    // Services/BookingResult.cs
    public record BookingResult(
        string TrackingNumber,
        string CarrierCode,
        DateTimeOffset EstimatedDelivery);
}
