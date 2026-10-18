using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Manufacturing.Persistence.Entities;

namespace Ultramaverick.Manufacturing.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("OutboxMessages", "Manufacturing");
        b.HasKey(x => x.EventId);

        b.Property(x => x.AggregateType).HasMaxLength(200).IsRequired();
        b.Property(x => x.AggregateId).HasMaxLength(100).IsRequired();
        b.Property(x => x.EventType).HasMaxLength(200).IsRequired();
        b.Property(x => x.Payload).IsRequired();
        b.Property(x => x.LastError).HasMaxLength(2000);

        b.HasIndex(x => new { x.PublishedAtUtc, x.OccurredAtUtc });
    }
}
