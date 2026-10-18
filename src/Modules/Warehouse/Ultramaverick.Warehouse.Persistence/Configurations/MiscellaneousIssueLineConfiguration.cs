using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Persistence.Configurations;

public sealed class MiscellaneousIssueLineConfiguration : IEntityTypeConfiguration<MiscellaneousIssueLine>
{
    public void Configure(EntityTypeBuilder<MiscellaneousIssueLine> b)
    {
        b.ToTable("MiscellaneousIssueLines", "Warehouse");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.LotNo).HasMaxLength(100);
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.Property(x => x.Remarks).HasMaxLength(2000);

        b.HasIndex(x => x.MiscellaneousIssueId);

        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
