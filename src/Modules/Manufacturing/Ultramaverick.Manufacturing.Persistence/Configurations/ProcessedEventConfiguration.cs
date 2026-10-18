using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Manufacturing.Persistence.Entities;

namespace Ultramaverick.Manufacturing.Persistence.Configurations;

public sealed class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> b)
    {
        b.ToTable("ProcessedEvents", "Manufacturing");
        b.HasKey(x => new { x.EventId, x.ProjectionName });

        b.Property(x => x.ProjectionName).HasMaxLength(200);
    }
}
