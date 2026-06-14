using FulfillmentService.Domain;
using System.Reflection.Emit;

namespace FulfillmentService.Repositories
{
    // Repositories/IShipmentRepository.cs
    public interface IShipmentRepository
    {
        Task<Shipment?> GetByIdAsync(Guid shipmentId, CancellationToken ct);
        Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct);
        Task SaveAsync(Shipment shipment, CancellationToken ct);
        Task UpdateAsync(Shipment shipment, CancellationToken ct);
    }
}
