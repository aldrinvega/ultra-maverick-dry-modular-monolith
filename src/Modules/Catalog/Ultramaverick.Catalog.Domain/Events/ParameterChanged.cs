using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Events;

public static class ParameterChangeTypes
{
    public const string Created = nameof(Created);
    public const string Updated = nameof(Updated);
    public const string Activated = nameof(Activated);
    public const string Deactivated = nameof(Deactivated);
}

public sealed record ParameterChanged(string ChangeType, DateTime OccurredAtUtc) : IDomainEvent;