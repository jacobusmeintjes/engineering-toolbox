using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain;

namespace OrderService.Repositories.Configurations
{    
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .ValueGeneratedNever();      // we set the Guid, not the DB

            builder.Property(o => o.CustomerId)
                .IsRequired();

            builder.Property(o => o.Status)
                .IsRequired()
                .HasConversion<string>()     // store as "Confirmed" not 3
                .HasMaxLength(40);

            builder.Property(o => o.ShippingAddress)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(o => o.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.PlacedAt)
                .IsRequired();

            builder.Property(o => o.PaymentTransactionId)
                .HasMaxLength(100);

            builder.Property(o => o.ShipmentId)
                .HasMaxLength(100);

            builder.HasMany(o => o.LineItems)
                .WithOne()
                .HasForeignKey(li => li.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    // Repositories/Configurations/OrderLineItemConfiguration.cs
    public class OrderLineItemConfiguration : IEntityTypeConfiguration<OrderLineItem>
    {
        public void Configure(EntityTypeBuilder<OrderLineItem> builder)
        {
            builder.ToTable("OrderLineItems");

            builder.HasKey(li => li.Id);

            builder.Property(li => li.Id)
                .ValueGeneratedNever();

            builder.Property(li => li.OrderId)
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

            builder.Property(li => li.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
        }
    }
}
