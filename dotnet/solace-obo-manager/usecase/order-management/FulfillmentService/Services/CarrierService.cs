using FulfillmentService.Domain;

namespace FulfillmentService.Services
{
    // Services/CarrierService.cs  — stub, replace with DHL/UPS/FedEx SDK
    public class CarrierService(ILogger<CarrierService> logger) : ICarrierService
    {
        public Task<BookingResult> BookCollectionAsync(
            Shipment shipment,
            CancellationToken ct)
        {
            // Replace with real carrier API call
            logger.LogInformation(
                "Booking collection for shipment {ShipmentId}", shipment.Id);

            return Task.FromResult(new BookingResult(
                TrackingNumber: $"TRK{Random.Shared.Next(100000000, 999999999)}",
                CarrierCode: "DHL",
                EstimatedDelivery: DateTimeOffset.UtcNow.AddBusinessDays(3)));
        }
    }
}
