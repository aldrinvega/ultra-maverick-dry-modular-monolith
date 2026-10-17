using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Persistence.Configurations;

public sealed class FormulaRequirementConfiguration : IEntityTypeConfiguration<FormulaRequirement>
{
    public void Configure(EntityTypeBuilder<FormulaRequirement> b)
    {
        b.ToTable("FormulaRequirements", "Catalog");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.HasIndex(x => new { x.FormulaHeaderId, x.RawMaterialId }).IsUnique();

        // The header -> requirement relationship is declared on FormulaHeaderConfiguration.
        b.HasOne<Item>().WithMany().HasForeignKey(x => x.RawMaterialId).OnDelete(DeleteBehavior.Restrict);

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}