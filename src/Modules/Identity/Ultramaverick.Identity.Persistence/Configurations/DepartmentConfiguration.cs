using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Persistence.Configurations;

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments", "Identity");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name).HasMaxLength(150).IsRequired();
        builder.HasIndex(d => d.Name).IsUnique();

        builder.Property(d => d.CreatedAtUtc).IsRequired();
        builder.Property(d => d.IsActive).IsRequired();
        builder.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
