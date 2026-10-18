namespace Ultramaverick.Reporting.Domain;

/// <summary>The status values carried by the reporting read models.</summary>
public static class ReportingStatuses
{
    public const string Created = nameof(Created);
    public const string Confirmed = nameof(Confirmed);
    public const string Cancelled = nameof(Cancelled);
    public const string Completed = nameof(Completed);
}
