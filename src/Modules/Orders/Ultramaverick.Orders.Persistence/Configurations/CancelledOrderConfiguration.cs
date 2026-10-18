using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Orders.Domain.Entities;

namespace Ultramaverick.Orders.Persistence.Configurations;

public sealed class CancelledOrderConfiguration : IEntityTypeConfiguration<CancelledOrder>
{
    public void Configure(EntityTypeBuilder<CancelledOrder> b)
    {
        b.ToTable("CancelledOrders", "Orders");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.HasIndex(x => x.OrderId);

        b.HasOne<Order>().WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);

        // Map-only and never written this wave: no RowVersion.
    }
}
