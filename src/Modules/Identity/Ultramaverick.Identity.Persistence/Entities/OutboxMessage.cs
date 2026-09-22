namespace Ultramaverick.Identity.Persistence.Entities
{
    public sealed class OutboxMessage
    {
        public Guid EventId { get; private set; }
        public DateTime OccurredAtUtc { get; private set; }
        public string AggregateType { get; private set; } = null!;
        public string AggregateId { get; private set; } = null!;
        public string EventType { get; private set; } = null!;
        public long Version { get; private set; }
        public string Payload { get; private set; } = null!;
        public DateTime? PublishedAtUtc { get; private set; }
        public int Attempts { get; private set; }
        public string? LastError { get; private set; }

        private OutboxMessage() { }

        public static OutboxMessage Create(string aggregateType, string aggregateId, string eventType, string payload) => new()
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = DateTime.UtcNow,
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            EventType = eventType,
            Version = 1,
            Payload = payload,
            Attempts = 0
        };
    }
}
