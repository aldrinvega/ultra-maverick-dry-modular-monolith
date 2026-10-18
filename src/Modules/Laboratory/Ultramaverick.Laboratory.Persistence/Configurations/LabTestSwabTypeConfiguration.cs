using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.Persistence.Configurations;

public sealed class LabTestSwabTypeConfiguration : IEntityTypeConfiguration<LabTestSwabType>
{
    public void Configure(EntityTypeBuilder<LabTestSwabType> b)
    {
        b.ToTable("LabTestSwabTypes", "Laboratory");
        b.HasKey(x => new { x.LabTestRequestId, x.SwabTypeId });
    }
}
