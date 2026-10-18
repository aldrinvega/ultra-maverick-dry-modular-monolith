using Ultramaverick.Quality.Persistence.Lookups;

namespace Ultramaverick.Quality.IntegrationTests;

public class PurchaseOrderLineLookupTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public PurchaseOrderLineLookupTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetAsync_reads_the_purchase_order_line_cross_schema()
    {
        int lineId;
        await using (var procurement = _fixture.CreateProcurementContext())
        {
            (_, lineId) = await TestData.CreatePurchaseOrderWithLineAsync(procurement);
        }

        await using var context = _fixture.CreateContext();
        var lookup = new PurchaseOrderLineLookup(context);

        var info = await lookup.GetAsync(lineId, CancellationToken.None);

        Assert.NotNull(info);
        Assert.Equal(lineId, info!.PurchaseOrderLineId);
        Assert.Equal(10, info.ItemId);
        Assert.Equal(20, info.UomId);
        Assert.Equal(5m, info.Quantity);
        // Neither column exists yet in Procurement/Catalog, so the port projects NULL.
        Assert.Null(info.ProductTypeId);
        Assert.Null(info.BatchNo);
    }

    [Fact]
    public async Task GetAsync_returns_null_for_an_unknown_line()
    {
        await using var context = _fixture.CreateContext();
        var lookup = new PurchaseOrderLineLookup(context);

        Assert.Null(await lookup.GetAsync(987654, CancellationToken.None));
    }
}
