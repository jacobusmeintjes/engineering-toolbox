using FulfillmentService.Domain;

namespace FulfillmentService.Services
{
    // Services/IWarehouseSystem.cs
    public interface IWarehouseSystem
    {
        Task<PickListResult> CreatePickListAsync(
            Guid shipmentId,
            IReadOnlyList<ShipmentLineItem> items,
            CancellationToken ct);
    }
}
