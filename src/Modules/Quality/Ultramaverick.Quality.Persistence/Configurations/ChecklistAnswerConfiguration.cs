using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Quality.Domain.Entities;

namespace Ultramaverick.Quality.Persistence.Configurations;

public sealed class ChecklistAnswerConfiguration : IEntityTypeConfiguration<ChecklistAnswer>
{
    public void Configure(EntityTypeBuilder<ChecklistAnswer> b)
    {
        b.ToTable("ChecklistAnswers", "Quality");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.HasIndex(x => new { x.ChecklistId, x.QuestionId }).IsUnique();

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
