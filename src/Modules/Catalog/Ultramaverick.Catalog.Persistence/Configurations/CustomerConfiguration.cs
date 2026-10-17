using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.ToTable("Customers", "Catalog");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.CustomerCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        b.Property(x => x.CompanyCode).HasMaxLength(50);
        b.Property(x => x.CompanyName).HasMaxLength(200);
        b.Property(x => x.MobileNumber).HasMaxLength(30);
        b.Property(x => x.LeadMan).HasMaxLength(150);
        b.Property(x => x.Address).HasMaxLength(300);
        b.HasIndex(x => x.CustomerCode).IsUnique();

        b.HasOne<Farm>().WithMany().HasForeignKey(x => x.FarmId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<AccountTitle>().WithMany().HasForeignKey(x => x.AccountTitleId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Location>().WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);

        // DepartmentId has NO relationship: it is a scalar cross-context reference.
        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}