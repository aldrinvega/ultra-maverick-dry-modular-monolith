using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Orders.Domain.Entities;

namespace Ultramaverick.Orders.Persistence.Configurations;

public sealed class MoveOrderConfiguration : IEntityTypeConfiguration<MoveOrder>
{
    public void Configure(EntityTypeBuilder<MoveOrder> b)
    {
        b.ToTable("MoveOrders", "Orders");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.QuantityOrdered).HasPrecision(18, 4);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.PlateNumber).HasMaxLength(50);
        b.Property(x => x.DeliveryStatus).HasMaxLength(50);
        b.Property(x => x.CompanyCode).HasMaxLength(50);
        b.Property(x => x.CompanyName).HasMaxLength(200);
        b.Property(x => x.Remarks).HasMaxLength(1000);
        b.Property(x => x.IsActive).IsRequired();

        b.HasIndex(x => x.OrderId);
        b.HasIndex(x => x.ItemId);

        b.HasOne<Order>().WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<AdvanceToEmployee>().WithMany()
            .HasForeignKey(x => x.AdvancesToEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
