using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.Persistence.Configurations;

public sealed class LabTestParameterConfiguration : IEntityTypeConfiguration<LabTestParameter>
{
    public void Configure(EntityTypeBuilder<LabTestParameter> b)
    {
        b.ToTable("LabTestParameters", "Laboratory");
        b.HasKey(x => new { x.LabTestRequestId, x.ParameterId });
    }
}
