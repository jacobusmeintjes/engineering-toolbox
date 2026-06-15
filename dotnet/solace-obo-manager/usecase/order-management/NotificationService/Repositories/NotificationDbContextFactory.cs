using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NotificationService.Repositories
{
    // Repositories/NotificationDbContextFactory.cs
    public class NotificationDbContextFactory
        : IDesignTimeDbContextFactory<NotificationDbContext>
    {
        public NotificationDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var options = new DbContextOptionsBuilder<NotificationDbContext>()
                .UseNpgsql(config.GetConnectionString("NotificationDb"))
                .Options;

            return new NotificationDbContext(options);
        }
    }
}
