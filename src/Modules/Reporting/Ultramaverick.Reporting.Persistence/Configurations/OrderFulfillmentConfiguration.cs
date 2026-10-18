using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Reporting.Domain.Entities;

namespace Ultramaverick.Reporting.Persistence.Configurations;

public sealed class OrderFulfillmentConfiguration : IEntityTypeConfiguration<OrderFulfillment>
{
    public void Configure(EntityTypeBuilder<OrderFulfillment> b)
    {
        b.ToTable("OrderFulfillment", "Reporting");
        b.HasKey(x => x.OrderNumber);

        b.Property(x => x.OrderNumber).HasMaxLength(64).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}
