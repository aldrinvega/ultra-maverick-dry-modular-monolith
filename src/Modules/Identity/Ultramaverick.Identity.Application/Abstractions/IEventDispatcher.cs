namespace Ultramaverick.Identity.Application.Abstractions
{
    /// <summary>
    /// A handler for one published event type. Each module implements the events it consumes.
    /// Handlers must be idempotent; the dispatcher additionally guards them with the
    /// processed-events table keyed by (EventId, handler type name).
    /// </summary>
    public interface IIntegrationEventHandler
    {
        /// <summary>The EventType value as stored on the outbox row (for example "UserChanged").</summary>
        string EventType { get; }

        Task HandleAsync(OutboxRecord record, CancellationToken ct);
    }

    public interface IEventDispatcher
    {
        Task DispatchAsync(OutboxRecord record, CancellationToken ct);
    }

    public interface IProcessedEventStore
    {
        Task<bool> HasProcessedAsync(Guid eventId, string projection, CancellationToken ct);
        Task MarkProcessedAsync(Guid eventId, string projection, CancellationToken ct);
    }
}
