using Ultramaverick.Catalog.Domain.Entities;
using Ultramaverick.Procurement.Persistence.Lookups;

namespace Ultramaverick.Procurement.IntegrationTests;

public class SupplierLookupTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public SupplierLookupTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task ExistsAsync_returns_false_for_an_unknown_supplier()
    {
        await using var context = _fixture.CreateContext();
        var lookup = new SupplierLookup(context);

        Assert.False(await lookup.ExistsAsync(987654, CancellationToken.None));
    }

    [Fact]
    public async Task ExistsAsync_returns_true_for_an_active_supplier()
    {
        var supplierId = await SeedSupplierAsync(active: true);

        await using var context = _fixture.CreateContext();
        var lookup = new SupplierLookup(context);

        Assert.True(await lookup.ExistsAsync(supplierId, CancellationToken.None));
    }

    [Fact]
    public async Task ExistsAsync_returns_false_for_an_inactive_supplier()
    {
        var supplierId = await SeedSupplierAsync(active: false);

        await using var context = _fixture.CreateContext();
        var lookup = new SupplierLookup(context);

        Assert.False(await lookup.ExistsAsync(supplierId, CancellationToken.None));
    }

    private async Task<int> SeedSupplierAsync(bool active)
    {
        await using var catalog = _fixture.CreateCatalogContext();

        var supplier = Supplier.Create(Code("SUP"), "Test Supplier", null, null);
        if (!active) supplier.SetActive(false, 1);

        catalog.Suppliers.Add(supplier);
        await catalog.SaveChangesAsync();

        return supplier.Id;
    }

    private static string Code(string prefix) => $"{prefix}{Guid.NewGuid():N}"[..12];
}
