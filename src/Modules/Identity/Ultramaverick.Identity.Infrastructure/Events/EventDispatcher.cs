using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Identity.Infrastructure.Events;

/// <summary>
/// Finds the IIntegrationEventHandler implementations registered for a record's EventType and
/// invokes each. Idempotency is consumer-owned; the dispatcher keeps no processed-events guard.
/// </summary>
public sealed class EventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _provider;

    public EventDispatcher(IServiceProvider provider) => _provider = provider;

    public async Task DispatchAsync(OutboxRecord record, CancellationToken ct)
    {
        foreach (var handler in _provider.GetServices<IIntegrationEventHandler>())
        {
            if (!string.Equals(handler.EventType, record.EventType, StringComparison.Ordinal))
                continue;

            await handler.HandleAsync(record, ct);
        }
    }
}
