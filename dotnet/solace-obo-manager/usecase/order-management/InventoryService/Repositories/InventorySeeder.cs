using global::InventoryService.Domain;
using InventoryService.Domain;

namespace InventoryService.Repositories
{


    public static class InventorySeeder
    {
        public static async Task SeedAsync(InventoryDbContext context)
        {
            if (context.StockItems.Any())
            {
                return; // Already seeded
            }

            var stockItems = new[]
            {
                StockItem.Create(sku: "SKU-LAPTOP-001", productName: "Dell XPS 13", quantity:100),
                StockItem.Create(sku : "SKU-MOUSE-001",  productName: "Logitech MX Master 3", quantity : 500),      
                StockItem.Create(sku: "SKU-KEYBOARD-001", productName: "Mechanical RGB Keyboard", quantity: 250 )
            };

            context.StockItems.AddRange(stockItems);
            await context.SaveChangesAsync();
        }
    }
}
