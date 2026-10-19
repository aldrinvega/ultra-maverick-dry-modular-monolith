namespace Ultramaverick.Manufacturing.Contracts;

/// <summary>The change kinds carried by a transformation domain event. The change type travels
/// in the payload because the event never carries an id (the outbox row owns the aggregate id).</summary>
public static class TransformationChangeTypes
{
    public const string Created = nameof(Created);
    public const string Prepared = nameof(Prepared);
    public const string Rejected = nameof(Rejected);
    public const string Cancelled = nameof(Cancelled);
}


