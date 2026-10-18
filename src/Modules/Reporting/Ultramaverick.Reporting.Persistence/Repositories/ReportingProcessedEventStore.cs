using Microsoft.EntityFrameworkCore;
using Ultramaverick.Reporting.Persistence.Entities;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Reporting.Persistence.Repositories;

/// <summary>
/// Reporting's consumer-owned idempotency marker. It commits in one transaction with the
/// projection write because both go through the same DbContext.
/// </summary>
public sealed class ReportingProcessedEventStore : IProcessedEventStore
{
    private readonly ReportingDbContext _context;

    public ReportingProcessedEventStore(ReportingDbContext context) => _context = context;

    public Task<bool> HasProcessedAsync(Guid eventId, string projection, CancellationToken ct)
        => _context.ProcessedEvents
            .AsNoTracking()
            .AnyAsync(p => p.EventId == eventId && p.ProjectionName == projection, ct);

    public async Task MarkProcessedAsync(Guid eventId, string projection, CancellationToken ct)
        => await _context.ProcessedEvents.AddAsync(ProcessedEvent.Create(eventId, projection), ct);
}
