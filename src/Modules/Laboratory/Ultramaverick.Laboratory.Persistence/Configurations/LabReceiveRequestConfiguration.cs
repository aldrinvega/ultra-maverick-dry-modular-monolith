using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.Persistence.Configurations;

public sealed class LabReceiveRequestConfiguration : IEntityTypeConfiguration<LabReceiveRequest>
{
    public void Configure(EntityTypeBuilder<LabReceiveRequest> b)
    {
        b.ToTable("LabReceiveRequests", "Laboratory");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.Status).HasMaxLength(50).IsRequired();
        b.Property(x => x.Remarks).HasMaxLength(2000);

        // Non-unique on purpose: a test request may be received more than once.
        b.HasIndex(x => x.LabTestRequestId);

        b.HasOne<LabTestRequest>()
            .WithMany()
            .HasForeignKey(x => x.LabTestRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
