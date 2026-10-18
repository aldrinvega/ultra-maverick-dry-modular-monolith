using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Identity.Infrastructure.Events;

/// <summary>
/// Finds the IIntegrationEventHandler implementations registered for a record's EventType and
/// invokes each. Idempotency is consumer-owned; the dispatcher keeps no processed-events guard.
/// </summary>
public sealed class EventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<EventDispatcher> _logger;

    public EventDispatcher(IServiceProvider provider, ILogger<EventDispatcher> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public async Task DispatchAsync(OutboxRecord record, CancellationToken ct)
    {
        var matched = false;

        foreach (var handler in _provider.GetServices<IIntegrationEventHandler>())
        {
            if (!string.Equals(handler.EventType, record.EventType, StringComparison.Ordinal))
                continue;

            matched = true;
            await handler.HandleAsync(record, ct);
        }

        if (!matched)
            _logger.LogWarning(
                "No handler registered for event type {EventType}; {EventId} will be marked published without processing.",
                record.EventType, record.EventId);
    }
}
