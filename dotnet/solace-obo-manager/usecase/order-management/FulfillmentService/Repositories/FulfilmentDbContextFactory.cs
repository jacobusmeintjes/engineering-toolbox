using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FulfillmentService.Repositories
{
    // Repositories/FulfilmentDbContextFactory.cs
    public class FulfilmentDbContextFactory : IDesignTimeDbContextFactory<FulfilmentDbContext>
    {
        public FulfilmentDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var options = new DbContextOptionsBuilder<FulfilmentDbContext>()
                .UseNpgsql(config.GetConnectionString("FulfilmentDb"))
                .Options;

            return new FulfilmentDbContext(options);
        }
    }
}
