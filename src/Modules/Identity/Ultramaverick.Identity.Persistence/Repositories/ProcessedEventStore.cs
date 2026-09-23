using Microsoft.EntityFrameworkCore;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Persistence.Entities;

namespace Ultramaverick.Identity.Persistence.Repositories;

public sealed class ProcessedEventStore : IProcessedEventStore
{
    private readonly IdentityDbContext _context;

    public ProcessedEventStore(IdentityDbContext context) => _context = context;

    public Task<bool> HasProcessedAsync(Guid eventId, string projection, CancellationToken ct)
        => _context.ProcessedEvents
            .AnyAsync(p => p.EventId == eventId && p.ProjectionName == projection, ct);

    public async Task MarkProcessedAsync(Guid eventId, string projection, CancellationToken ct)
    {
        if (await HasProcessedAsync(eventId, projection, ct)) return;

        _context.ProcessedEvents.Add(ProcessedEvent.Create(eventId, projection));
        await _context.SaveChangesAsync(ct);
    }
}
