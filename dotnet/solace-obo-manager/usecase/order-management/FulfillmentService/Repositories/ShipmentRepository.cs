using FulfillmentService.Domain;
using Microsoft.EntityFrameworkCore;

namespace FulfillmentService.Repositories
{
    // Repositories/ShipmentRepository.cs
    public class ShipmentRepository(FulfilmentDbContext db) : IShipmentRepository
    {
        public async Task<Shipment?> GetByIdAsync(Guid shipmentId, CancellationToken ct) =>
            await db.Shipments
                .Include(s => s.LineItems)
                .FirstOrDefaultAsync(s => s.Id == shipmentId, ct);

        public async Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct) =>
            await db.Shipments
                .Include(s => s.LineItems)
                .FirstOrDefaultAsync(s => s.OrderId == orderId, ct);

        public async Task SaveAsync(Shipment shipment, CancellationToken ct)
        {
            await db.Shipments.AddAsync(shipment, ct);
            await db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Shipment shipment, CancellationToken ct)
        {
            db.Shipments.Update(shipment);
            await db.SaveChangesAsync(ct);
        }
    }
}
