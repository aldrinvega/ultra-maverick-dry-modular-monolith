using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Persistence.Configurations;

public sealed class LotNameConfiguration : IEntityTypeConfiguration<LotName>
{
    public void Configure(EntityTypeBuilder<LotName> b)
    {
        b.ToTable("LotNames", "Catalog");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.SectionName).HasMaxLength(100);
        b.HasIndex(x => x.Code).IsUnique();

        b.HasOne<LotCategory>().WithMany().HasForeignKey(x => x.LotCategoryId).OnDelete(DeleteBehavior.Restrict);

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}