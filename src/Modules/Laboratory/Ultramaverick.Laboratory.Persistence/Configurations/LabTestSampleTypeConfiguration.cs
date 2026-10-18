using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.Persistence.Configurations;

public sealed class LabTestSampleTypeConfiguration : IEntityTypeConfiguration<LabTestSampleType>
{
    public void Configure(EntityTypeBuilder<LabTestSampleType> b)
    {
        b.ToTable("LabTestSampleTypes", "Laboratory");
        b.HasKey(x => new { x.LabTestRequestId, x.SampleTypeId });
    }
}
