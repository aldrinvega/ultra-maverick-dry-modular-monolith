using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Quality.Domain.Entities;

namespace Ultramaverick.Quality.Persistence.Configurations;

public sealed class ChecklistReviewVerificationLogConfiguration : IEntityTypeConfiguration<ChecklistReviewVerificationLog>
{
    public void Configure(EntityTypeBuilder<ChecklistReviewVerificationLog> b)
    {
        b.ToTable("ChecklistReviewVerificationLogs", "Quality");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.Disposition).HasMaxLength(100).IsRequired();
        b.Property(x => x.Remarks).HasMaxLength(2000);
        b.HasIndex(x => x.ChecklistId);

        b.HasOne<Checklist>()
            .WithMany()
            .HasForeignKey(x => x.ChecklistId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Property(x => x.ReviewedAtUtc).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
