using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Quality.Domain.Entities;

namespace Ultramaverick.Quality.Persistence.Configurations;

public sealed class QcReceiptConfiguration : IEntityTypeConfiguration<QcReceipt>
{
    public void Configure(EntityTypeBuilder<QcReceipt> b)
    {
        b.ToTable("QcReceipts", "Quality");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.BatchNo).HasMaxLength(100);
        b.Property(x => x.Status).HasMaxLength(50).IsRequired();
        b.Property(x => x.CancelRemarks).HasMaxLength(2000);
        b.Property(x => x.ActualDelivered).HasPrecision(18, 4);
        b.Property(x => x.TotalReject).HasPrecision(18, 4);
        b.HasIndex(x => x.PurchaseOrderId);

        b.HasMany(x => x.Rejects)
            .WithOne()
            .HasForeignKey(r => r.QcReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Rejects).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(x => x.Checklists)
            .WithOne()
            .HasForeignKey(c => c.QcReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Checklists).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
