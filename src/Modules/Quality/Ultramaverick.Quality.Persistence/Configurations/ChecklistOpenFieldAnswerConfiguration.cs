using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Quality.Domain.Entities;

namespace Ultramaverick.Quality.Persistence.Configurations;

public sealed class ChecklistOpenFieldAnswerConfiguration : IEntityTypeConfiguration<ChecklistOpenFieldAnswer>
{
    public void Configure(EntityTypeBuilder<ChecklistOpenFieldAnswer> b)
    {
        b.ToTable("ChecklistOpenFieldAnswers", "Quality");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.Remarks).HasMaxLength(2000).IsRequired();
        b.HasIndex(x => x.ChecklistId);

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.IsActive).IsRequired();
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
