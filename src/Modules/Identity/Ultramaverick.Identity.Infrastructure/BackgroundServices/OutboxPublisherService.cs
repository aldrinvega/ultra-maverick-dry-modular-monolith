using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Identity.Infrastructure.BackgroundServices;

/// <summary>
/// Polls every registered IOutboxStore (one per producing module) for unpublished rows,
/// dispatches them through the event dispatcher, then marks them published. Failures are
/// recorded (Attempts/LastError) and retried on the next pass; handlers are idempotent,
/// so redelivery is safe.
/// </summary>
public sealed class OutboxPublisherService : BackgroundService
{
    private const int DefaultBatchSize = 50;
    private static readonly TimeSpan DefaultPollInterval = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPublisherService> _logger;
    private readonly int _batchSize;
    private readonly TimeSpan _pollInterval;

    public OutboxPublisherService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxPublisherService> logger,
        IConfiguration? configuration = null)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _batchSize = configuration?.GetValue("Outbox:BatchSize", DefaultBatchSize) ?? DefaultBatchSize;
        _pollInterval = TimeSpan.FromSeconds(
            configuration?.GetValue("Outbox:PollSeconds", DefaultPollInterval.TotalSeconds)
            ?? DefaultPollInterval.TotalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
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
                await Task.Delay(_pollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Drains every registered outbox store exactly once. Public so tests can drive a single
    /// deterministic pass without waiting on the polling interval.
    /// </summary>
    public async Task RunOnceAsync(CancellationToken ct)
    {
        using var storeScope = _scopeFactory.CreateScope();

        var stores = storeScope.ServiceProvider.GetServices<IOutboxStore>();

        foreach (var store in stores)
        {
            // A fault in one store must not starve the stores registered after it.
            try
            {
                var pending = await store.GetPendingAsync(_batchSize, ct);

                foreach (var record in pending)
                {
                    // A failed projection can leave its DbContext tracking detached entities.
                    // Isolating each event keeps one poison row from failing the rest of the batch.
                    using var eventScope = _scopeFactory.CreateScope();
                    var dispatcher = eventScope.ServiceProvider.GetRequiredService<IEventDispatcher>();

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox store {Store} failed during a drain pass.", store.GetType().FullName);
            }
        }
    }
}
