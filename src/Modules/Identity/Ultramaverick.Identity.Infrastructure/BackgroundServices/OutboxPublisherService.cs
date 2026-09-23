using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ultramaverick.Identity.Application.Abstractions;

namespace Ultramaverick.Identity.Infrastructure.BackgroundServices;

/// <summary>
/// Polls Infrastructure.OutboxMessages for unpublished rows, dispatches them through the
/// event dispatcher, then marks them published. Failures are recorded (Attempts/LastError)
/// and retried on the next pass; handlers are idempotent, so redelivery is safe.
/// </summary>
public sealed class OutboxPublisherService : BackgroundService
{
    private const int BatchSize = 50;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPublisherService> _logger;

    public OutboxPublisherService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxPublisherService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox publisher iteration failed.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task PublishPendingAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();

        var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IEventDispatcher>();

        var pending = await store.GetPendingAsync(BatchSize, ct);

        foreach (var record in pending)
        {
            try
            {
                await dispatcher.DispatchAsync(record, ct);
                await store.MarkPublishedAsync(record.EventId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to publish {EventType} ({EventId}); attempt {Attempts}.",
                    record.EventType, record.EventId, record.Attempts + 1);

                await store.MarkFailedAsync(record.EventId, ex.Message, ct);
            }
        }
    }
}
