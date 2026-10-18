using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Orders.Domain.Entities;

namespace Ultramaverick.Orders.Persistence.Configurations;

public sealed class AdvanceToEmployeeConfiguration : IEntityTypeConfiguration<AdvanceToEmployee>
{
    public void Configure(EntityTypeBuilder<AdvanceToEmployee> b)
    {
        b.ToTable("AdvancesToEmployees", "Orders");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.EmployeeName).HasMaxLength(200).IsRequired();

        // Map-only and never written this wave: no RowVersion.
    }
}
