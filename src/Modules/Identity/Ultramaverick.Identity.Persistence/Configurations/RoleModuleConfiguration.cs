using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Persistence.Configurations;

public sealed class RoleModuleConfiguration : IEntityTypeConfiguration<RoleModule>
{
    public void Configure(EntityTypeBuilder<RoleModule> builder)
    {
        builder.ToTable("RoleModules", "Identity");
        builder.HasKey(rm => new { rm.RoleId, rm.ModuleId });

        builder.Property(rm => rm.CreatedAtUtc).IsRequired();
        builder.Property(rm => rm.IsActive).IsRequired();

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(rm => rm.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Module>()
            .WithMany()
            .HasForeignKey(rm => rm.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
