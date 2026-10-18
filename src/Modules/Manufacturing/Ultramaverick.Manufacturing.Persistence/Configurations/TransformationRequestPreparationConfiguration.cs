using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Manufacturing.Domain.Entities;

namespace Ultramaverick.Manufacturing.Persistence.Configurations;

public sealed class TransformationRequestPreparationConfiguration
    : IEntityTypeConfiguration<TransformationRequestPreparation>
{
    public void Configure(EntityTypeBuilder<TransformationRequestPreparation> b)
    {
        b.ToTable("TransformationRequestPreparations", "Manufacturing");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.QuantityNeeded).HasPrecision(18, 4).IsRequired();
        b.Property(x => x.WeighingScale).HasMaxLength(50);

        b.HasIndex(x => x.TransformationRequestId);
    }
}
