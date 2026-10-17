using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Persistence.Configurations;

public sealed class FormulaHeaderConfiguration : IEntityTypeConfiguration<FormulaHeader>
{
    public void Configure(EntityTypeBuilder<FormulaHeader> b)
    {
        b.ToTable("FormulaHeaders", "Catalog");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.HasIndex(x => new { x.ItemId, x.Version }).IsUnique();

        b.HasOne<Item>().WithMany().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Requirements).WithOne().HasForeignKey(r => r.FormulaHeaderId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Requirements).HasField("_requirements").UsePropertyAccessMode(PropertyAccessMode.Field);

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}