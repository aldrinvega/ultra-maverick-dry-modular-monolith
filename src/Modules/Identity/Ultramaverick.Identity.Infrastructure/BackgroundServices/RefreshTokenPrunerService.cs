using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ultramaverick.Identity.Application.Abstractions;

namespace Ultramaverick.Identity.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically deletes refresh tokens that can no longer be used and are past the
/// retention window, so the table does not grow without bound.
/// </summary>
public sealed class RefreshTokenPrunerService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);
    private static readonly TimeSpan Retention = TimeSpan.FromDays(7);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RefreshTokenPrunerService> _logger;

    public RefreshTokenPrunerService(
        IServiceScopeFactory scopeFactory,
        ILogger<RefreshTokenPrunerService> logger)
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
                using var scope = _scopeFactory.CreateScope();
                var store = scope.ServiceProvider.GetRequiredService<IRefreshTokenStore>();

                var removed = await store.PruneAsync(DateTime.UtcNow - Retention, stoppingToken);

                if (removed > 0)
                    _logger.LogInformation("Pruned {Count} stale refresh tokens.", removed);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh token pruning failed.");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
