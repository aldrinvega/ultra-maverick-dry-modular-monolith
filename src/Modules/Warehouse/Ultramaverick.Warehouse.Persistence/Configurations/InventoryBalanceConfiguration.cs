using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Persistence.Configurations;

public sealed class InventoryBalanceConfiguration : IEntityTypeConfiguration<InventoryBalance>
{
    public void Configure(EntityTypeBuilder<InventoryBalance> b)
    {
        b.ToTable("InventoryBalances", "Warehouse");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.LotNo).HasMaxLength(100);
        b.Property(x => x.OnHand).HasPrecision(18, 4);

        b.HasIndex(x => new { x.WarehouseId, x.ItemId, x.UomId, x.LotNo }).IsUnique();

        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
