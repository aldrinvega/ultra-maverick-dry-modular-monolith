using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Events;

public static class SupplierChangeTypes
{
    public const string Created = nameof(Created);
    public const string Updated = nameof(Updated);
    public const string Activated = nameof(Activated);
    public const string Deactivated = nameof(Deactivated);
}

public sealed record SupplierChanged(string ChangeType, DateTime OccurredAtUtc) : IDomainEvent;