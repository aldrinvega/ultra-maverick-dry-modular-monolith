using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Reporting.Domain.Entities;

namespace Ultramaverick.Reporting.Persistence.Configurations;

public sealed class InventoryPositionConfiguration : IEntityTypeConfiguration<InventoryPosition>
{
    public void Configure(EntityTypeBuilder<InventoryPosition> b)
    {
        b.ToTable("InventoryPositions", "Reporting");
        b.HasKey(x => new { x.WarehouseId, x.ItemId, x.LotNo });

        b.Property(x => x.LotNo).HasMaxLength(50).IsRequired();
        b.Property(x => x.Quantity).HasPrecision(18, 4).IsRequired();
    }
}
