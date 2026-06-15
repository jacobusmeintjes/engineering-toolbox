using FulfillmentService.Domain;
using Microsoft.EntityFrameworkCore;

namespace FulfillmentService.Repositories
{
    // Repositories/FulfilmentDbContext.cs
    public class FulfilmentDbContext(DbContextOptions<FulfilmentDbContext> options)
        : DbContext(options)
    {
        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<ShipmentLineItem> ShipmentLineItems => Set<ShipmentLineItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FulfilmentDbContext).Assembly);
        }
    }
}
