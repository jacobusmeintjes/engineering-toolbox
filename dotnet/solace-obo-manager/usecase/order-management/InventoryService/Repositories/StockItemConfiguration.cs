using InventoryService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryService.Repositories
{
    // Repositories/Configurations/StockItemConfiguration.cs
    public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
    {
        public void Configure(EntityTypeBuilder<StockItem> builder)
        {
            builder.ToTable("StockItems");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .ValueGeneratedNever();

            builder.Property(s => s.Sku)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.TotalQuantity)
                .IsRequired();

            builder.Property(s => s.ReservedQuantity)
                .IsRequired();

            builder.Ignore(s => s.AvailableQuantity);   // computed, not persisted

            builder.HasIndex(s => s.Sku)
                .IsUnique();
        }
    }
}
