using InventoryService.Domain;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Repositories
{
    // Repositories/InventoryDbContext.cs
    public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
    {
        public DbSet<StockItem> StockItems => Set<StockItem>();
        public DbSet<StockReservationRecord> Reservations => Set<StockReservationRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
        }
    }
}
