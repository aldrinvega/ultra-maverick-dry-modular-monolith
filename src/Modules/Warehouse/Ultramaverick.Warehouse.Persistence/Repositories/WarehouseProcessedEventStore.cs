using Microsoft.EntityFrameworkCore;
using Ultramaverick.SharedKernel.Outbox;
using Ultramaverick.Warehouse.Persistence.Entities;

namespace Ultramaverick.Warehouse.Persistence.Repositories;

/// <summary>
/// Warehouse's own consumer-owned idempotency marker. The marker is added through the same
/// DbContext as the aggregates, so it commits in one transaction with them (atomicity).
/// </summary>
public sealed class WarehouseProcessedEventStore : IProcessedEventStore
{
    private readonly WarehouseDbContext _context;

    public WarehouseProcessedEventStore(WarehouseDbContext context) => _context = context;

    public Task<bool> HasProcessedAsync(Guid eventId, string projection, CancellationToken ct)
        => _context.ProcessedEvents
            .AsNoTracking()
            .AnyAsync(p => p.EventId == eventId && p.ProjectionName == projection, ct);

    public async Task MarkProcessedAsync(Guid eventId, string projection, CancellationToken ct)
        => await _context.ProcessedEvents.AddAsync(ProcessedEvent.Create(eventId, projection), ct);
}
