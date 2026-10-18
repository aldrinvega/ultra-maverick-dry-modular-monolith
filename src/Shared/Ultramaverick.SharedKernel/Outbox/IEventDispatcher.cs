namespace Ultramaverick.SharedKernel.Outbox
{
    /// <summary>
    /// A handler for one published event type. Each module implements the events it consumes.
    /// Idempotency is owned by the consumer and must be applied atomically with its own side
    /// effects; the dispatcher keeps no processed-events guard.
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
