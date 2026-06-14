using Microsoft.EntityFrameworkCore;
using OrderService.Domain;
using System.Reflection.Emit;

namespace OrderService.Repositories
{
    // Repositories/OrderDbContext.cs
    public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderLineItem> LineItems => Set<OrderLineItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
        }
    }
}
