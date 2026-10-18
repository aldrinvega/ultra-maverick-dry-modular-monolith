using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Orders.IntegrationTests;

/// <summary>
/// Composes every module's service registration into one provider (as Program.cs does) and
/// asserts Ordering's registrations coexist without conflict: a per-module outbox store and
/// Ordering's consumer-owned processed-event store.
/// </summary>
public class DependencyInjectionTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public DependencyInjectionTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public void Every_module_contributes_its_own_outbox_store()
    {
        using var scope = _fixture.CreateScope();

        var stores = scope.ServiceProvider.GetServices<IOutboxStore>().ToList();

        Assert.Contains(stores, s => s.GetType().FullName!.Contains(".Orders."));
        Assert.Contains(stores, s => s.GetType().FullName!.Contains(".Warehouse."));
        Assert.Contains(stores, s => s.GetType().FullName!.Contains(".Quality."));
        Assert.Contains(stores, s => s.GetType().FullName!.Contains(".Procurement."));
        Assert.Contains(stores, s => s.GetType().FullName!.Contains(".Catalog."));
        Assert.Contains(stores, s => s.GetType().FullName!.Contains(".Identity."));
    }

    [Fact]
    public void Orders_registers_its_processed_event_store()
    {
        using var scope = _fixture.CreateScope();

        var stores = scope.ServiceProvider.GetServices<IProcessedEventStore>()
            .Where(s => s.GetType().FullName!.Contains(".Orders."))
            .ToList();

        Assert.Single(stores);
    }
}
