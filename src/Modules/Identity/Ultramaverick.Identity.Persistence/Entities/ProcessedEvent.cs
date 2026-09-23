namespace Ultramaverick.Identity.Persistence.Entities
{
    /// <summary>
    /// Records that a given event has already been handled by a given projection,
    /// making event handling idempotent across retries and restarts.
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
