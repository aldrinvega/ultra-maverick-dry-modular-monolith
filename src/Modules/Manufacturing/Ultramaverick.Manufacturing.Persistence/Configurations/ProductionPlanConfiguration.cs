using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Manufacturing.Domain.Entities;

namespace Ultramaverick.Manufacturing.Persistence.Configurations;

public sealed class ProductionPlanConfiguration : IEntityTypeConfiguration<ProductionPlan>
{
    public void Configure(EntityTypeBuilder<ProductionPlan> b)
    {
        b.ToTable("ProductionPlans", "Manufacturing");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.ProdPlanNo).HasMaxLength(50);
        b.Property(x => x.Batch).HasMaxLength(100);
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.RejectRemarks).HasMaxLength(1000);
        b.Property(x => x.CancelRemarks).HasMaxLength(1000);

        b.HasIndex(x => x.ItemId);

        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
