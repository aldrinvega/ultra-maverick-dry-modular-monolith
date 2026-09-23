using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Identity.Persistence.Entities;

namespace Ultramaverick.Identity.Persistence.Configurations;

public sealed class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        builder.ToTable("ProcessedEvents", "Infrastructure");
        builder.HasKey(p => new { p.EventId, p.ProjectionName });

        builder.Property(p => p.ProjectionName).HasMaxLength(128).IsRequired();
        builder.Property(p => p.ProcessedAtUtc).IsRequired();
    }
}
