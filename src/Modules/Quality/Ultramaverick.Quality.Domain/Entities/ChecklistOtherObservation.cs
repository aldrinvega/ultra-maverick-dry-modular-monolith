using Ultramaverick.SharedKernel;

namespace Ultramaverick.Quality.Domain.Entities;

public sealed class ChecklistOtherObservation : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int ChecklistId { get; private set; }
    public string Observation { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private ChecklistOtherObservation() { }

    internal static ChecklistOtherObservation Create(int checklistId, string text)
        => new() { ChecklistId = checklistId, Observation = text, IsActive = true };
}
