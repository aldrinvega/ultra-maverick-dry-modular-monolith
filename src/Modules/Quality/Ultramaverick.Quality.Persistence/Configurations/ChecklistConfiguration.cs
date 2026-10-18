using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Quality.Domain.Entities;

namespace Ultramaverick.Quality.Persistence.Configurations;

public sealed class ChecklistConfiguration : IEntityTypeConfiguration<Checklist>
{
    public void Configure(EntityTypeBuilder<Checklist> b)
    {
        b.ToTable("Checklists", "Quality");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.HasIndex(x => x.QcReceiptId);

        b.HasMany(x => x.Answers)
            .WithOne()
            .HasForeignKey(a => a.ChecklistId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Answers).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(x => x.OpenFields)
            .WithOne()
            .HasForeignKey(o => o.ChecklistId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.OpenFields).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(x => x.Dimensions)
            .WithOne()
            .HasForeignKey(d => d.ChecklistId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Dimensions).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(x => x.Observations)
            .WithOne()
            .HasForeignKey(o => o.ChecklistId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Observations).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(x => x.Compliances)
            .WithOne()
            .HasForeignKey(c => c.ChecklistId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Compliances).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
