using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Quality.Domain.Entities;

namespace Ultramaverick.Quality.Persistence.Configurations;

public sealed class ChecklistTypeConfiguration : IEntityTypeConfiguration<ChecklistType>
{
    public void Configure(EntityTypeBuilder<ChecklistType> b)
    {
        b.ToTable("ChecklistTypes", "Quality");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.Name).HasMaxLength(150).IsRequired();

        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
