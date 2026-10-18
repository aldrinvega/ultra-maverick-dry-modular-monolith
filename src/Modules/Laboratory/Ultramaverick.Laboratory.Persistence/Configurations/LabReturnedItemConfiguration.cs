using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.Persistence.Configurations;

public sealed class LabReturnedItemConfiguration : IEntityTypeConfiguration<LabReturnedItem>
{
    public void Configure(EntityTypeBuilder<LabReturnedItem> b)
    {
        b.ToTable("LabReturnedItems", "Laboratory");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.Quantity).HasPrecision(18, 4);
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
