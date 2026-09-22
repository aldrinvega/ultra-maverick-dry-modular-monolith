using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Ultramaverick.Identity.Persistence.Entities;

namespace Ultramaverick.Identity.Persistence.Configurations
{
    public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages", "Infrastructure");
            builder.HasKey(o => o.EventId);
            builder.Property(o => o.AggregateType).IsRequired().HasMaxLength(128);
            builder.Property(o => o.AggregateId).IsRequired().HasMaxLength(64);
            builder.Property(o => o.EventType).IsRequired().HasMaxLength(256);
            builder.Property(o => o.Payload).IsRequired();
            builder.Property(o => o.LastError).HasMaxLength(2000);
            builder.HasIndex(o => new { o.PublishedAtUtc, o.OccurredAtUtc })
                .HasFilter("[PublishedAtUtc] IS NULL");
        }
    }
}
