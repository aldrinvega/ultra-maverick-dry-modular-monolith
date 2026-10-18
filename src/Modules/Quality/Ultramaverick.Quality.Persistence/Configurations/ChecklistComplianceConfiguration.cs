using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Quality.Domain.Entities;

namespace Ultramaverick.Quality.Persistence.Configurations;

public sealed class ChecklistComplianceConfiguration : IEntityTypeConfiguration<ChecklistCompliance>
{
    public void Configure(EntityTypeBuilder<ChecklistCompliance> b)
    {
        b.ToTable("ChecklistCompliances", "Quality");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.Compliance).HasMaxLength(150).IsRequired();
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.RootCause).HasMaxLength(2000);
        b.HasIndex(x => x.ChecklistId);

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
