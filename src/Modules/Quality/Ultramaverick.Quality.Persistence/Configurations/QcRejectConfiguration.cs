using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Quality.Domain.Entities;

namespace Ultramaverick.Quality.Persistence.Configurations;

public sealed class QcRejectConfiguration : IEntityTypeConfiguration<QcReject>
{
    public void Configure(EntityTypeBuilder<QcReject> b)
    {
        b.ToTable("QcRejects", "Quality");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.Property(x => x.Remarks).HasMaxLength(2000);
        b.HasIndex(x => x.QcReceiptId);

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
