using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.Persistence.Configurations;

public sealed class LabTestProductConditionConfiguration : IEntityTypeConfiguration<LabTestProductCondition>
{
    public void Configure(EntityTypeBuilder<LabTestProductCondition> b)
    {
        b.ToTable("LabTestProductConditions", "Laboratory");
        b.HasKey(x => new { x.LabTestRequestId, x.ProductConditionId });
    }
}
