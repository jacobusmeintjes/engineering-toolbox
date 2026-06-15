using FulfillmentService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FulfillmentService.Repositories
{
    // Repositories/Configurations/ShipmentLineItemConfiguration.cs
    public class ShipmentLineItemConfiguration : IEntityTypeConfiguration<ShipmentLineItem>
    {
        public void Configure(EntityTypeBuilder<ShipmentLineItem> builder)
        {
            builder.ToTable("ShipmentLineItems");

            builder.HasKey(li => li.Id);

            builder.Property(li => li.Id)
                .ValueGeneratedNever();

            builder.Property(li => li.ShipmentId)
                .IsRequired();

            builder.Property(li => li.ProductId)
                .IsRequired();

            builder.Property(li => li.Sku)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(li => li.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(li => li.Quantity)
                .IsRequired();

            builder.HasIndex(li => li.ShipmentId);
        }
    }
}
