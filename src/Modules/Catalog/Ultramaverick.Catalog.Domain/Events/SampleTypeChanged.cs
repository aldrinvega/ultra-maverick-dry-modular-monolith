using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Events;

public static class SampleTypeChangeTypes
{
    public const string Created = nameof(Created);
    public const string Updated = nameof(Updated);
    public const string Activated = nameof(Activated);
    public const string Deactivated = nameof(Deactivated);
}

public sealed record SampleTypeChanged(string ChangeType, DateTime OccurredAtUtc) : IDomainEvent;