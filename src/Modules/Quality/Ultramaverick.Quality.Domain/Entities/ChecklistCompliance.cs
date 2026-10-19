using Ultramaverick.SharedKernel;

namespace Ultramaverick.Quality.Domain.Entities;

public sealed class ChecklistCompliance : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int ChecklistId { get; private set; }
    public string Compliance { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? RootCause { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private ChecklistCompliance() { }

    internal static ChecklistCompliance Create(int checklistId, string compliance, string? description, string? rootCause)
        => new()
        {
            ChecklistId = checklistId,
            Compliance = compliance,
            Description = description,
            RootCause = rootCause,
            IsActive = true
        };
}
