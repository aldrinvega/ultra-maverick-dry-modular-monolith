using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Reporting.Persistence.Entities;

namespace Ultramaverick.Reporting.Persistence.Configurations;

public sealed class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> b)
    {
        b.ToTable("ProcessedEvents", "Reporting");
        b.HasKey(x => new { x.EventId, x.ProjectionName });

        b.Property(x => x.ProjectionName).HasMaxLength(200);
    }
}
