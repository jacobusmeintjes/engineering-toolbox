using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain;

namespace NotificationService.Repositories
{
    // Repositories/Configurations/NotificationRecordConfiguration.cs
    public class NotificationRecordConfiguration : IEntityTypeConfiguration<NotificationRecord>
    {
        public void Configure(EntityTypeBuilder<NotificationRecord> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.Id)
                .ValueGeneratedNever();

            builder.Property(n => n.CustomerId)
                .IsRequired();

            builder.Property(n => n.EventType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(n => n.OrderId)
                .IsRequired();

            builder.Property(n => n.Channel)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(10);

            builder.Property(n => n.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(10);

            builder.Property(n => n.FailureReason)
                .HasMaxLength(500);

            builder.Property(n => n.SentAt)
                .IsRequired();

            builder.HasIndex(n => n.OrderId);
            builder.HasIndex(n => n.CustomerId);
        }
    }
}
