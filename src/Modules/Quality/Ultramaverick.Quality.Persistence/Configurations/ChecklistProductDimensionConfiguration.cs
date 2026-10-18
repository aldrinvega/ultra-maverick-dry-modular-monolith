using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Quality.Domain.Entities;

namespace Ultramaverick.Quality.Persistence.Configurations;

public sealed class ChecklistProductDimensionConfiguration : IEntityTypeConfiguration<ChecklistProductDimension>
{
    public void Configure(EntityTypeBuilder<ChecklistProductDimension> b)
    {
        b.ToTable("ChecklistProductDimensions", "Quality");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.StandardValue).HasPrecision(18, 4);
        b.Property(x => x.ActualValue).HasPrecision(18, 4);
        b.HasIndex(x => x.ChecklistId);

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
