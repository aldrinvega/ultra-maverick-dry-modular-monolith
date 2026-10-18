namespace Ultramaverick.Laboratory.Domain;

/// <summary>The status values carried by Laboratory aggregates. Kept as constants so the
/// magic strings live in one place, mirroring the change-type convention.</summary>
public static class LaboratoryStatuses
{
    public const string Requested = nameof(Requested);
    public const string Accepted = nameof(Accepted);
    public const string ForQaApproval = nameof(ForQaApproval);
    public const string Approved = nameof(Approved);
}
