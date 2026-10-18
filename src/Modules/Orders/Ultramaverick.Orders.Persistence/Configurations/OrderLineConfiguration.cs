using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Orders.Domain.Entities;

namespace Ultramaverick.Orders.Persistence.Configurations;

public sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> b)
    {
        b.ToTable("OrderLines", "Orders");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.OriginalQuantityOrdered).HasPrecision(18, 4);
        b.Property(x => x.QuantityOrdered).HasPrecision(18, 4);
        b.Property(x => x.AllocatedQuantity).HasPrecision(18, 4);
        b.Property(x => x.PlateNumber).HasMaxLength(50);
        b.Property(x => x.DeliveryStatus).HasMaxLength(50);
        b.Property(x => x.IsActive).IsRequired();

        b.HasIndex(x => x.OrderId);
        b.HasIndex(x => x.ItemId);

        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
