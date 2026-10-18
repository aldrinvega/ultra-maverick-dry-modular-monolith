using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Manufacturing.Domain.Entities;

namespace Ultramaverick.Manufacturing.Persistence.Configurations;

public sealed class TransformationRequestRejectConfiguration
    : IEntityTypeConfiguration<TransformationRequestReject>
{
    public void Configure(EntityTypeBuilder<TransformationRequestReject> b)
    {
        b.ToTable("TransformationRequestRejects", "Manufacturing");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.Quantity).HasPrecision(18, 4).IsRequired();

        b.HasIndex(x => x.TransformationRequestId);
    }
}
