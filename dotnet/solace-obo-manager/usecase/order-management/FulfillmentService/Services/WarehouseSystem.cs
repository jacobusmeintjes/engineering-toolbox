using FulfillmentService.Domain;

namespace FulfillmentService.Services
{
    // Services/WarehouseSystem.cs  — stub, replace with real WMS integration
    public class WarehouseSystem(ILogger<WarehouseSystem> logger) : IWarehouseSystem
    {
        public Task<PickListResult> CreatePickListAsync(
            Guid shipmentId,
            IReadOnlyList<ShipmentLineItem> items,
            CancellationToken ct)
        {
            // Replace with real WMS API call
            logger.LogInformation(
                "Creating pick list for shipment {ShipmentId} with {Count} items",
                shipmentId, items.Count);

            return Task.FromResult(new PickListResult(
                PickListId: $"PL-{Guid.NewGuid():N}"[..11], //Guid.NewGuid().ToString().Substring(0,8)
                WarehouseLocation: "ZONE-A"));
        }
    }
}
