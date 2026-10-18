using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Persistence.Configurations;

public sealed class WarehouseReceiptConfiguration : IEntityTypeConfiguration<WarehouseReceipt>
{
    public void Configure(EntityTypeBuilder<WarehouseReceipt> b)
    {
        b.ToTable("WarehouseReceipts", "Warehouse");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.LotNo).HasMaxLength(100);
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.Property(x => x.IsActive).IsRequired();

        // A QC receipt maps to at most one warehouse receipt; manual receipts carry a null
        // QcReceiptId, so the uniqueness only applies to non-null values.
        b.HasIndex(x => x.QcReceiptId).IsUnique().HasFilter("[QcReceiptId] IS NOT NULL");
        b.HasIndex(x => x.WarehouseId);

        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
