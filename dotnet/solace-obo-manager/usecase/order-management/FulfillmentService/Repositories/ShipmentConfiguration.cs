using FulfillmentService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FulfillmentService.Repositories
{
    // Repositories/Configurations/ShipmentConfiguration.cs
    public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
    {
        public void Configure(EntityTypeBuilder<Shipment> builder)
        {
            builder.ToTable("Shipments");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .ValueGeneratedNever();

            builder.Property(s => s.OrderId)
                .IsRequired();

            builder.Property(s => s.ShippingAddress)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(s => s.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(s => s.TrackingNumber)
                .HasMaxLength(100);

            builder.Property(s => s.CarrierCode)
                .HasMaxLength(20);

            builder.Property(s => s.CreatedAt)
                .IsRequired();

            builder.Property(s => s.EstimatedDelivery)
                .IsRequired();

            builder.HasMany(s => s.LineItems)
                .WithOne()
                .HasForeignKey(li => li.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(s => s.OrderId);
            builder.HasIndex(s => s.TrackingNumber);
        }
    }
}
