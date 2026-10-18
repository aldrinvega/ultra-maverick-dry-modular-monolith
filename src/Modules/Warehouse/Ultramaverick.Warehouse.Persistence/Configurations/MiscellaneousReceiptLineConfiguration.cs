using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Persistence.Configurations;

public sealed class MiscellaneousReceiptLineConfiguration : IEntityTypeConfiguration<MiscellaneousReceiptLine>
{
    public void Configure(EntityTypeBuilder<MiscellaneousReceiptLine> b)
    {
        b.ToTable("MiscellaneousReceiptLines", "Warehouse");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.LotNo).HasMaxLength(100);
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.Property(x => x.Remarks).HasMaxLength(2000);

        b.HasIndex(x => x.MiscellaneousReceiptId);

        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
