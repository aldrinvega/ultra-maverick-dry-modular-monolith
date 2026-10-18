using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Quality.IntegrationTests;

/// <summary>
/// Composes every module's service registration into one provider (as Program.cs does) and
/// asserts Quality's registrations coexist without conflict: exactly one Quality event handler,
/// a per-module outbox store, and Quality's consumer-owned processed-event store.
/// </summary>
public class DependencyInjectionTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public DependencyInjectionTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public void Quality_registers_exactly_one_PurchaseOrderReceived_handler()
    {
        using var scope = _fixture.CreateScope();

        var qualityHandlers = scope.ServiceProvider.GetServices<IIntegrationEventHandler>()
            .Where(h => h.GetType().FullName!.Contains(".Quality."))
            .ToList();

        var handler = Assert.Single(qualityHandlers);
        Assert.Equal("PurchaseOrderReceived", handler.EventType);
    }

    [Fact]
    public void Every_module_contributes_its_own_outbox_store()
    {
        using var scope = _fixture.CreateScope();

        var stores = scope.ServiceProvider.GetServices<IOutboxStore>().ToList();

        Assert.Contains(stores, s => s.GetType().FullName!.Contains(".Quality."));
        Assert.Contains(stores, s => s.GetType().FullName!.Contains(".Procurement."));
        Assert.Contains(stores, s => s.GetType().FullName!.Contains(".Catalog."));
        Assert.Contains(stores, s => s.GetType().FullName!.Contains(".Identity."));
    }

    [Fact]
    public void Quality_registers_its_processed_event_store()
    {
        using var scope = _fixture.CreateScope();

        var qualityStores = scope.ServiceProvider.GetServices<IProcessedEventStore>()
            .Where(s => s.GetType().FullName!.Contains(".Quality."))
            .ToList();

        Assert.Single(qualityStores);
    }
}
