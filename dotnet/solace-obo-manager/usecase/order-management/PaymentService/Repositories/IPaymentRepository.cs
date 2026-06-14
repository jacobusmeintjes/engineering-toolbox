using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Domain;

namespace PaymentService.Repositories
{
    // Repositories/IPaymentRepository.cs
    public interface IPaymentRepository
    {
        Task<PaymentRecord?> GetByTransactionIdAsync(string transactionId, CancellationToken ct);
        Task<PaymentRecord?> GetByOrderIdAsync(Guid orderId, CancellationToken ct);
        Task SaveAsync(PaymentRecord record, CancellationToken ct);
        Task UpdateAsync(PaymentRecord record, CancellationToken ct);
    }

    // Repositories/PaymentRepository.cs
    public class PaymentRepository(PaymentDbContext db) : IPaymentRepository
    {
        public async Task<PaymentRecord?> GetByTransactionIdAsync(
            string transactionId, CancellationToken ct) =>
            await db.Payments
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId, ct);

        public async Task<PaymentRecord?> GetByOrderIdAsync(
            Guid orderId, CancellationToken ct) =>
            await db.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId, ct);

        public async Task SaveAsync(PaymentRecord record, CancellationToken ct)
        {
            await db.Payments.AddAsync(record, ct);
            await db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(PaymentRecord record, CancellationToken ct)
        {
            db.Payments.Update(record);
            await db.SaveChangesAsync(ct);
        }
    }

    // Repositories/PaymentDbContext.cs
    public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
    {
        public DbSet<PaymentRecord> Payments => Set<PaymentRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
        }
    }

    // Repositories/Configurations/PaymentRecordConfiguration.cs
    public class PaymentRecordConfiguration : IEntityTypeConfiguration<PaymentRecord>
    {
        public void Configure(EntityTypeBuilder<PaymentRecord> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedNever();

            builder.Property(p => p.OrderId)
                .IsRequired();

            builder.Property(p => p.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(p => p.TransactionId)
                .HasMaxLength(100);

            builder.Property(p => p.FailureReason)
                .HasMaxLength(500);

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.HasIndex(p => p.OrderId);
            builder.HasIndex(p => p.TransactionId);
        }
    }
}
