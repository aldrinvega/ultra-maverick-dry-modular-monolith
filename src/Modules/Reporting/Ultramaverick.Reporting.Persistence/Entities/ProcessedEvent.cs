namespace Ultramaverick.Reporting.Persistence.Entities
{
    /// <summary>
    /// Records that a given event has already been applied by a given projection, making the
    /// projections idempotent across dispatcher retries and restarts. This table is the module's
    /// projection checkpoint store.
    /// </summary>
    public sealed class ProcessedEvent
    {
        public Guid EventId { get; private set; }
        public string ProjectionName { get; private set; } = null!;
        public DateTime ProcessedAtUtc { get; private set; }

        private ProcessedEvent() { }

        public static ProcessedEvent Create(Guid eventId, string projectionName) => new()
        {
            EventId = eventId,
            ProjectionName = projectionName,
            ProcessedAtUtc = DateTime.UtcNow
        };
    }
}
