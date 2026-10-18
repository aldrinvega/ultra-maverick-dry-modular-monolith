using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Manufacturing.Domain.Entities;

namespace Ultramaverick.Manufacturing.Persistence.Configurations;

public sealed class TransformationRequestLineConfiguration : IEntityTypeConfiguration<TransformationRequestLine>
{
    public void Configure(EntityTypeBuilder<TransformationRequestLine> b)
    {
        b.ToTable("TransformationRequestLines", "Manufacturing");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.RequiredQuantity).HasPrecision(18, 4).IsRequired();

        b.HasIndex(x => x.TransformationRequestId);
    }
}
