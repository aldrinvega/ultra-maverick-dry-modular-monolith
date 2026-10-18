using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Persistence.Configurations;

public sealed class MiscellaneousIssueConfiguration : IEntityTypeConfiguration<MiscellaneousIssue>
{
    public void Configure(EntityTypeBuilder<MiscellaneousIssue> b)
    {
        b.ToTable("MiscellaneousIssues", "Warehouse");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.RefNo).HasMaxLength(100);
        b.Property(x => x.Remarks).HasMaxLength(2000);
        b.Property(x => x.IsActive).IsRequired();

        b.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(l => l.MiscellaneousIssueId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
