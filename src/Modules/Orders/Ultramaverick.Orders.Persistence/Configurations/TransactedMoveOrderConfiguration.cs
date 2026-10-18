using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Orders.Domain.Entities;

namespace Ultramaverick.Orders.Persistence.Configurations;

public sealed class TransactedMoveOrderConfiguration : IEntityTypeConfiguration<TransactedMoveOrder>
{
    public void Configure(EntityTypeBuilder<TransactedMoveOrder> b)
    {
        b.ToTable("TransactedMoveOrders", "Orders");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.IsTransact).IsRequired();

        b.HasIndex(x => x.MoveOrderId);

        b.HasOne<MoveOrder>().WithMany().HasForeignKey(x => x.MoveOrderId).OnDelete(DeleteBehavior.Restrict);

        // Immutable: no RowVersion by design.
    }
}
