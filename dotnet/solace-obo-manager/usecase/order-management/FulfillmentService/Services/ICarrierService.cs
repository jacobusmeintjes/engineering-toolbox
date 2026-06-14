using FulfillmentService.Domain;

namespace FulfillmentService.Services
{
    // Services/ICarrierService.cs
    public interface ICarrierService
    {
        Task<BookingResult> BookCollectionAsync(
            Shipment shipment,
            CancellationToken ct);
    }
}
