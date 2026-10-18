using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.Persistence.Configurations;

public sealed class LabTestRequestConfiguration : IEntityTypeConfiguration<LabTestRequest>
{
    public void Configure(EntityTypeBuilder<LabTestRequest> b)
    {
        b.ToTable("LabTestRequests", "Laboratory");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.BatchNo).HasMaxLength(100);
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.Property(x => x.TestType).HasMaxLength(100);
        b.Property(x => x.Status).HasMaxLength(50).IsRequired();
        b.Property(x => x.Remarks).HasMaxLength(2000);

        b.HasMany(x => x.Analyses)
            .WithOne()
            .HasForeignKey(x => x.LabTestRequestId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Analyses).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(x => x.Parameters)
            .WithOne()
            .HasForeignKey(x => x.LabTestRequestId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Parameters).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(x => x.ProductConditions)
            .WithOne()
            .HasForeignKey(x => x.LabTestRequestId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.ProductConditions).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(x => x.SampleTypes)
            .WithOne()
            .HasForeignKey(x => x.LabTestRequestId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.SampleTypes).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(x => x.SwabTypes)
            .WithOne()
            .HasForeignKey(x => x.LabTestRequestId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.SwabTypes).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
