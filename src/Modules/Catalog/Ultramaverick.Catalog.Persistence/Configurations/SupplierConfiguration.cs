using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Persistence.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> b)
    {
        b.ToTable("Suppliers", "Catalog");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.SupplierCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.SupplierName).HasMaxLength(200).IsRequired();
        b.Property(x => x.SupplierAddress).HasMaxLength(300);
        b.HasIndex(x => x.SupplierCode).IsUnique();

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}