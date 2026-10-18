using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Manufacturing.Domain.Entities;

namespace Ultramaverick.Manufacturing.Persistence.Configurations;

public sealed class TransformationRequestConfiguration : IEntityTypeConfiguration<TransformationRequest>
{
    public void Configure(EntityTypeBuilder<TransformationRequest> b)
    {
        b.ToTable("TransformationRequests", "Manufacturing");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.Status).HasMaxLength(30).IsRequired();

        // A request belongs to exactly one plan in this module. Deleting a plan with requests
        // is refused rather than cascading away the request history.
        b.HasOne<ProductionPlan>()
            .WithMany()
            .HasForeignKey(x => x.ProductionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Children take no parent id: the FK is wired from the navigation on save.
        b.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(l => l.TransformationRequestId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(x => x.Preparations)
            .WithOne()
            .HasForeignKey(p => p.TransformationRequestId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Preparations).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(x => x.Rejects)
            .WithOne()
            .HasForeignKey(r => r.TransformationRequestId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Rejects).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasIndex(x => x.ProductionPlanId);

        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
