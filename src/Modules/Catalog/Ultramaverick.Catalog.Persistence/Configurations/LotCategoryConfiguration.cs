using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Persistence.Configurations;

public sealed class LotCategoryConfiguration : IEntityTypeConfiguration<LotCategory>
{
    public void Configure(EntityTypeBuilder<LotCategory> b)
    {
        b.ToTable("LotCategories", "Catalog");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.Code).HasMaxLength(50);
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}