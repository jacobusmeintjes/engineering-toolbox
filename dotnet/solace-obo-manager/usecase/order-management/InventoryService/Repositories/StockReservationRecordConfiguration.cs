using InventoryService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryService.Repositories
{
    // Repositories/Configurations/StockReservationRecordConfiguration.cs
    public class StockReservationRecordConfiguration : IEntityTypeConfiguration<StockReservationRecord>
    {
        public void Configure(EntityTypeBuilder<StockReservationRecord> builder)
        {
            builder.ToTable("StockReservations");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .ValueGeneratedNever();

            builder.Property(r => r.OrderId)
                .IsRequired();

            builder.Property(r => r.Sku)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(r => r.Quantity)
                .IsRequired();

            builder.Property(r => r.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            builder.HasIndex(r => r.OrderId);
            builder.HasIndex(r => r.Sku);
        }
    }
}
