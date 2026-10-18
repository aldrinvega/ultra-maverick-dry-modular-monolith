using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Orders.Domain.Entities;

namespace Ultramaverick.Orders.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("Orders", "Orders");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.OrderNumber).HasMaxLength(64).IsRequired();
        b.Property(x => x.CustomerPosition).HasMaxLength(200);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Remarks).HasMaxLength(1000);
        b.Property(x => x.IsActive).IsRequired();

        b.HasIndex(x => x.OrderNumber).IsUnique();
        b.HasIndex(x => x.CustomerId);

        // OrderLine.Create takes no parent id; the FK is wired from the navigation here.
        b.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(l => l.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
