using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Reflection.Emit;

namespace InventoryService.Repositories
{

    // Repositories/InventoryDbContextFactory.cs
    public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
    {
        public InventoryDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseNpgsql(config.GetConnectionString("InventoryDb"))
                .Options;

            return new InventoryDbContext(options);
        }
    }
}
