using Ultramaverick.SharedKernel;

namespace Ultramaverick.Orders.Domain.Entities;

/// <summary>Map-only placeholder for an employee advance/assignment. No behaviour this wave.</summary>
public sealed class AdvanceToEmployee : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int? UserId { get; private set; }                // scalar → Identity (no FK)
    public string EmployeeName { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private AdvanceToEmployee() { }
}
