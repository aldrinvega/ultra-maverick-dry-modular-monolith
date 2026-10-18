using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Quality.Domain.Entities;

namespace Ultramaverick.Quality.Persistence.Configurations;

public sealed class ChecklistQuestionConfiguration : IEntityTypeConfiguration<ChecklistQuestion>
{
    public void Configure(EntityTypeBuilder<ChecklistQuestion> b)
    {
        b.ToTable("ChecklistQuestions", "Quality");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.Question).HasMaxLength(500).IsRequired();
        b.HasIndex(x => x.ChecklistTypeId);

        b.HasOne<ChecklistType>()
            .WithMany()
            .HasForeignKey(x => x.ChecklistTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
