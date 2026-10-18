using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Laboratory.IntegrationTests;

/// <summary>
/// Composes every module's service registration into one provider (as Program.cs does) and
/// asserts Laboratory's registrations coexist without conflict: its own outbox store and
/// processed-event store, and no cross-module integration handler.
/// </summary>
public class DependencyInjectionTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public DependencyInjectionTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public void Laboratory_registers_exactly_one_outbox_store()
    {
        using var scope = _fixture.CreateScope();

        var stores = scope.ServiceProvider.GetServices<IOutboxStore>()
            .Where(s => s.GetType().FullName!.Contains(".Laboratory."))
            .ToList();

        Assert.Single(stores);
    }

    [Fact]
    public void Laboratory_registers_exactly_one_processed_event_store()
    {
        using var scope = _fixture.CreateScope();

        var stores = scope.ServiceProvider.GetServices<IProcessedEventStore>()
            .Where(s => s.GetType().FullName!.Contains(".Laboratory."))
            .ToList();

        Assert.Single(stores);
    }

    [Fact]
    public void Laboratory_registers_no_integration_event_handler()
    {
        using var scope = _fixture.CreateScope();

        var handlers = scope.ServiceProvider.GetServices<IIntegrationEventHandler>()
            .Where(h => h.GetType().FullName!.Contains(".Laboratory."))
            .ToList();

        Assert.Empty(handlers);
    }
}
