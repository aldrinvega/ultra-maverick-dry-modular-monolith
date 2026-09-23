using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Identity.Application.Abstractions;

namespace Ultramaverick.Identity.Infrastructure.Events;

/// <summary>
/// Finds the IIntegrationEventHandler implementations registered for a record's EventType,
/// invoking each at most once per (EventId, handler type) using the processed-events table.
/// </summary>
public sealed class EventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _provider;

    public EventDispatcher(IServiceProvider provider) => _provider = provider;

    public async Task DispatchAsync(OutboxRecord record, CancellationToken ct)
    {
        var processed = _provider.GetRequiredService<IProcessedEventStore>();

        foreach (var handler in _provider.GetServices<IIntegrationEventHandler>())
        {
            if (!string.Equals(handler.EventType, record.EventType, StringComparison.Ordinal))
                continue;

            var projection = handler.GetType().Name;

            if (await processed.HasProcessedAsync(record.EventId, projection, ct))
                continue;

            await handler.HandleAsync(record, ct);
            await processed.MarkProcessedAsync(record.EventId, projection, ct);
        }
    }
}
