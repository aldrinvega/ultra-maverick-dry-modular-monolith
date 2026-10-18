using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.Persistence.Configurations;

public sealed class LabTestAnalysisConfiguration : IEntityTypeConfiguration<LabTestAnalysis>
{
    public void Configure(EntityTypeBuilder<LabTestAnalysis> b)
    {
        b.ToTable("LabTestAnalyses", "Laboratory");
        b.HasKey(x => new { x.LabTestRequestId, x.AnalysisId });
    }
}
