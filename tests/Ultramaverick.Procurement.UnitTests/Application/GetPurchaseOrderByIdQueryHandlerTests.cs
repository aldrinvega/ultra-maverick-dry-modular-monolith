using Ultramaverick.Procurement.Application.Abstractions;
using Ultramaverick.Procurement.Application.Queries.PurchaseOrders;

namespace Ultramaverick.Procurement.UnitTests.Application;

internal sealed class FakePurchaseOrderReader : IPurchaseOrderReader
{
    public PurchaseOrderDetailDto? Result { get; set; }

    public Task<PurchaseOrderDetailDto?> GetByIdAsync(int purchaseOrderId, CancellationToken ct)
        => Task.FromResult(Result);
}

public class GetPurchaseOrderByIdQueryHandlerTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public async Task A_non_positive_id_is_a_failure(int id)
    {
        var handler = new GetPurchaseOrderByIdQueryHandler(new FakePurchaseOrderReader());

        var result = await handler.Handle(new GetPurchaseOrderByIdQuery(id), CancellationToken.None);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task A_missing_purchase_order_is_a_failure()
    {
        var handler = new GetPurchaseOrderByIdQueryHandler(new FakePurchaseOrderReader());

        var result = await handler.Handle(new GetPurchaseOrderByIdQuery(99), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Purchase order not found.", result.Error);
    }

    [Fact]
    public async Task An_existing_purchase_order_is_returned_with_its_lines()
    {
        var reader = new FakePurchaseOrderReader
        {
            Result = new PurchaseOrderDetailDto(
                99, "PO-99", "PR-1", null, null, 3, "Open", true,
                new[] { new PurchaseOrderLineDto(1, 10, 20, 5m, 0m, 0m, 12.5m, true) })
        };
        var handler = new GetPurchaseOrderByIdQueryHandler(reader);

        var result = await handler.Handle(new GetPurchaseOrderByIdQuery(99), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("PO-99", result.Value!.PoNumber);
        Assert.Single(result.Value.Lines);
    }
}
