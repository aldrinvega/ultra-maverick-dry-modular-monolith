namespace Ultramaverick.Identity.Application.Abstractions
{
    /// <summary>A pending outbox row, ready to be dispatched.</summary>
    public sealed record OutboxRecord(
        Guid EventId,
        string AggregateType,
        string AggregateId,
        string EventType,
        string Payload,
        int Attempts);

    public interface IOutboxStore
    {
        Task<IReadOnlyList<OutboxRecord>> GetPendingAsync(int batchSize, CancellationToken ct);
        Task MarkPublishedAsync(Guid eventId, CancellationToken ct);
        Task MarkFailedAsync(Guid eventId, string error, CancellationToken ct);
    }
}
