using Microsoft.EntityFrameworkCore;
using NotificationService.Domain;

namespace NotificationService.Repositories
{
    // Repositories/NotificationDbContext.cs
    public class NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : DbContext(options)
    {
        public DbSet<NotificationRecord> Notifications => Set<NotificationRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
        }
    }
}
