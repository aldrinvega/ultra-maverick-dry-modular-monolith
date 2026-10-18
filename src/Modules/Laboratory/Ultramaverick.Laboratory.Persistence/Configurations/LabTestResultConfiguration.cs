using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.Persistence.Configurations;

public sealed class LabTestResultConfiguration : IEntityTypeConfiguration<LabTestResult>
{
    public void Configure(EntityTypeBuilder<LabTestResult> b)
    {
        b.ToTable("LabTestResults", "Laboratory");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.Result).HasMaxLength(2000);
        b.Property(x => x.Status).HasMaxLength(50).IsRequired();
        b.Property(x => x.Remarks).HasMaxLength(2000);
        b.HasIndex(x => x.LabReceiveRequestId);

        b.HasOne<LabReceiveRequest>()
            .WithMany()
            .HasForeignKey(x => x.LabReceiveRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
