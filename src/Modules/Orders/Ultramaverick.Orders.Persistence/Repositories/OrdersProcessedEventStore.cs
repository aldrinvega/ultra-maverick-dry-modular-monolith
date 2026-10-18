using Microsoft.EntityFrameworkCore;
using Ultramaverick.Orders.Persistence.Entities;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Orders.Persistence.Repositories;

/// <summary>
/// Orders' own consumer-owned idempotency marker. The marker is added through the same
/// DbContext as the aggregates, so it commits in one transaction with them (atomicity).
/// </summary>
public sealed class OrdersProcessedEventStore : IProcessedEventStore
{
    private readonly OrdersDbContext _context;

    public OrdersProcessedEventStore(OrdersDbContext context) => _context = context;

    public Task<bool> HasProcessedAsync(Guid eventId, string projection, CancellationToken ct)
        => _context.ProcessedEvents
            .AsNoTracking()
            .AnyAsync(p => p.EventId == eventId && p.ProjectionName == projection, ct);

    public async Task MarkProcessedAsync(Guid eventId, string projection, CancellationToken ct)
        => await _context.ProcessedEvents.AddAsync(ProcessedEvent.Create(eventId, projection), ct);
}
