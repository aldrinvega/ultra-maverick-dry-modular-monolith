using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Persistence.Configurations;

public sealed class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.ToTable("Modules", "Identity");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name).HasMaxLength(150).IsRequired();
        builder.Property(m => m.SubMenuName).HasMaxLength(150);

        builder.Property(m => m.CreatedAtUtc).IsRequired();
        builder.Property(m => m.IsActive).IsRequired();
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasOne<MainMenu>()
            .WithMany()
            .HasForeignKey(m => m.MainMenuId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
