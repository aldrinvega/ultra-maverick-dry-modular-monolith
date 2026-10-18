using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Quality.Domain.Entities;

namespace Ultramaverick.Quality.Persistence.Configurations;

public sealed class ChecklistOtherObservationConfiguration : IEntityTypeConfiguration<ChecklistOtherObservation>
{
    public void Configure(EntityTypeBuilder<ChecklistOtherObservation> b)
    {
        b.ToTable("ChecklistOtherObservations", "Quality");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.Observation).HasMaxLength(2000).IsRequired();
        b.HasIndex(x => x.ChecklistId);

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
