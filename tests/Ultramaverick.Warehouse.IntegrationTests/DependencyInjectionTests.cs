using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Warehouse.IntegrationTests;

/// <summary>
/// Composes every module's service registration into one provider (as Program.cs does) and
/// asserts Warehouse's registrations coexist without conflict: exactly one Warehouse event
/// handler, a per-module outbox store, and Warehouse's consumer-owned processed-event store.
/// </summary>
public class DependencyInjectionTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public DependencyInjectionTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public void Warehouse_registers_a_handler_for_each_event_it_consumes()
    {
        using var scope = _fixture.CreateScope();

        var eventTypes = scope.ServiceProvider.GetServices<IIntegrationEventHandler>()
            .Where(h => h.GetType().FullName!.Contains(".Warehouse."))
            .Select(h => h.EventType)
            .OrderBy(t => t)
            .ToArray();

        Assert.Equal(new[] { "QcReceiptCompleted", "TransformationCompleted" }, eventTypes);
    }

    [Fact]
    public void Every_module_contributes_its_own_outbox_store()
    {
        using var scope = _fixture.CreateScope();

        var stores = scope.ServiceProvider.GetServices<IOutboxStore>().ToList();

        Assert.Contains(stores, s => s.GetType().FullName!.Contains(".Warehouse."));
        Assert.Contains(stores, s => s.GetType().FullName!.Contains(".Quality."));
        Assert.Contains(stores, s => s.GetType().FullName!.Contains(".Procurement."));
        Assert.Contains(stores, s => s.GetType().FullName!.Contains(".Catalog."));
        Assert.Contains(stores, s => s.GetType().FullName!.Contains(".Identity."));
    }

    [Fact]
    public void Warehouse_registers_its_processed_event_store()
    {
        using var scope = _fixture.CreateScope();

        var stores = scope.ServiceProvider.GetServices<IProcessedEventStore>()
            .Where(s => s.GetType().FullName!.Contains(".Warehouse."))
            .ToList();

        Assert.Single(stores);
    }
}
