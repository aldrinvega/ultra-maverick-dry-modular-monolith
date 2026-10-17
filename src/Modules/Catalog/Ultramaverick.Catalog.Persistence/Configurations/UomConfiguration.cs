using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Persistence.Configurations;

public sealed class UomConfiguration : IEntityTypeConfiguration<Uom>
{
    public void Configure(EntityTypeBuilder<Uom> b)
    {
        b.ToTable("Uoms", "Catalog");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.Code).HasMaxLength(20).IsRequired();
        b.Property(x => x.Description).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}