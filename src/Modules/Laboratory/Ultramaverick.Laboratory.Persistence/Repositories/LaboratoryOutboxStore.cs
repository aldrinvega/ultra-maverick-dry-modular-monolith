using Microsoft.EntityFrameworkCore;
using Ultramaverick.Laboratory.Persistence.Entities;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Laboratory.Persistence.Repositories;

public sealed class LaboratoryOutboxStore : IOutboxStore
{
    private readonly LaboratoryDbContext _context;

    public LaboratoryOutboxStore(LaboratoryDbContext context) => _context = context;

    public async Task<IReadOnlyList<OutboxRecord>> GetPendingAsync(int batchSize, CancellationToken ct)
    {
        var rows = await _context.OutboxMessages
            .AsNoTracking()
            .Where(m => m.PublishedAtUtc == null)
            .OrderBy(m => m.OccurredAtUtc)
            .Take(batchSize)
            .ToListAsync(ct);

        return rows
            .Select(m => new OutboxRecord(
                m.EventId, m.AggregateType, m.AggregateId, m.EventType, m.Payload, m.Attempts))
            .ToList();
    }

    public async Task MarkPublishedAsync(Guid eventId, CancellationToken ct)
    {
        await _context.OutboxMessages
            .Where(m => m.EventId == eventId && m.PublishedAtUtc == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(m => m.PublishedAtUtc, DateTime.UtcNow), ct);
    }

    public async Task MarkFailedAsync(Guid eventId, string error, CancellationToken ct)
    {
        await _context.OutboxMessages
            .Where(m => m.EventId == eventId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(m => m.Attempts, m => m.Attempts + 1)
                .SetProperty(m => m.LastError, error), ct);
    }
}
