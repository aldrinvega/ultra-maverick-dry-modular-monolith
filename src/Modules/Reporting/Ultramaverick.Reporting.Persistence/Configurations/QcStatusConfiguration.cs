using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Reporting.Domain.Entities;

namespace Ultramaverick.Reporting.Persistence.Configurations;

public sealed class QcStatusConfiguration : IEntityTypeConfiguration<QcStatus>
{
    public void Configure(EntityTypeBuilder<QcStatus> b)
    {
        b.ToTable("QcStatus", "Reporting");
        b.HasKey(x => x.QcReceiptId);

        b.Property(x => x.QcReceiptId).ValueGeneratedNever();

        b.Property(x => x.LotNo).HasMaxLength(50);
        b.Property(x => x.NetQuantity).HasPrecision(18, 4).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}
