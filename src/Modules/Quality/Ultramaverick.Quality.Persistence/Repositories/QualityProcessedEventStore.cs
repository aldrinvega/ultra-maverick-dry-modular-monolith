using Microsoft.EntityFrameworkCore;
using Ultramaverick.Quality.Persistence.Entities;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Quality.Persistence.Repositories;

/// <summary>
/// Quality's own consumer-owned idempotency marker. The marker is added through the same
/// DbContext as the receipts, so it commits in one transaction with them (atomicity).
/// </summary>
public sealed class QualityProcessedEventStore : IProcessedEventStore
{
    private readonly QualityDbContext _context;

    public QualityProcessedEventStore(QualityDbContext context) => _context = context;

    public Task<bool> HasProcessedAsync(Guid eventId, string projection, CancellationToken ct)
        => _context.ProcessedEvents
            .AsNoTracking()
            .AnyAsync(p => p.EventId == eventId && p.ProjectionName == projection, ct);

    public async Task MarkProcessedAsync(Guid eventId, string projection, CancellationToken ct)
        => await _context.ProcessedEvents.AddAsync(ProcessedEvent.Create(eventId, projection), ct);
}
