using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Persistence.Configurations;

public sealed class MiscellaneousReceiptConfiguration : IEntityTypeConfiguration<MiscellaneousReceipt>
{
    public void Configure(EntityTypeBuilder<MiscellaneousReceipt> b)
    {
        b.ToTable("MiscellaneousReceipts", "Warehouse");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.RefNo).HasMaxLength(100);
        b.Property(x => x.Remarks).HasMaxLength(2000);
        b.Property(x => x.IsActive).IsRequired();

        b.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(l => l.MiscellaneousReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
