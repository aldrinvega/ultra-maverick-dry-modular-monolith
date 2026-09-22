using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Persistence.Configurations;

public sealed class MainMenuConfiguration : IEntityTypeConfiguration<MainMenu>
{
    public void Configure(EntityTypeBuilder<MainMenu> builder)
    {
        builder.ToTable("MainMenus", "Identity");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name).HasMaxLength(150).IsRequired();
        builder.Property(m => m.Path).HasMaxLength(300).IsRequired();
        builder.Property(m => m.SortOrder).IsRequired();

        builder.Property(m => m.CreatedAtUtc).IsRequired();
        builder.Property(m => m.IsActive).IsRequired();
        builder.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
