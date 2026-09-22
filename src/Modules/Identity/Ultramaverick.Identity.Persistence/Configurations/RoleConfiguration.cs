using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Persistence.Configurations
{
    public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles", "Identity");
            builder.HasKey(r => r.Id);
            builder.Ignore(r => r.DomainEvents); 
            builder.Property(r => r.Name).HasMaxLength(100).IsRequired();
            builder.HasIndex(r => r.Name).IsUnique();
            builder.Property(r => r.IsActive).IsRequired();
            builder.Property(r => r.CreatedAtUtc).IsRequired();
            builder.Property<byte[]>("RowVersion").IsRowVersion();
        }
    }
}
