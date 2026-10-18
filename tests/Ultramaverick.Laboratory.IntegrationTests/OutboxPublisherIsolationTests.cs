using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ultramaverick.Identity.Infrastructure.BackgroundServices;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Laboratory.IntegrationTests;

/// <summary>
/// Proves the outbox publisher isolates per-store failures: a store whose read throws must not
/// starve the stores registered after it. Uses a hand-built container (no database).
/// </summary>
public class OutboxPublisherIsolationTests
{
    [Fact]
    public async Task A_throwing_store_does_not_stop_a_later_store_from_draining()
    {
        var healthy = new RecordingOutboxStore();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IEventDispatcher, NoOpEventDispatcher>();
        services.AddSingleton<IOutboxStore, ThrowingOutboxStore>();
        services.AddSingleton<IOutboxStore>(healthy);

        await using var provider = services.BuildServiceProvider();

        var publisher = new OutboxPublisherService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<ILogger<OutboxPublisherService>>());

        await publisher.RunOnceAsync(CancellationToken.None);

        Assert.True(healthy.WasPublished);
    }

    private sealed class ThrowingOutboxStore : IOutboxStore
    {
        public Task<IReadOnlyList<OutboxRecord>> GetPendingAsync(int batchSize, CancellationToken ct)
            => throw new InvalidOperationException("The store is unavailable.");

        public Task MarkPublishedAsync(Guid eventId, CancellationToken ct) => Task.CompletedTask;

        public Task MarkFailedAsync(Guid eventId, string error, CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class RecordingOutboxStore : IOutboxStore
    {
        public bool WasPublished { get; private set; }

        public Task<IReadOnlyList<OutboxRecord>> GetPendingAsync(int batchSize, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<OutboxRecord>>(new[]
            {
                new OutboxRecord(Guid.NewGuid(), "Aggregate", "1", "SomeEvent", "{}", Attempts: 0)
            });

        public Task MarkPublishedAsync(Guid eventId, CancellationToken ct)
        {
            WasPublished = true;
            return Task.CompletedTask;
        }

        public Task MarkFailedAsync(Guid eventId, string error, CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class NoOpEventDispatcher : IEventDispatcher
    {
        public Task DispatchAsync(OutboxRecord record, CancellationToken ct) => Task.CompletedTask;
    }
}
