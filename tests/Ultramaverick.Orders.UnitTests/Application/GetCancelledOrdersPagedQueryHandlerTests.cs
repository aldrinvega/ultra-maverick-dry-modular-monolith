using Ultramaverick.Orders.Application.Queries.CancelledOrders;

namespace Ultramaverick.Orders.UnitTests.Application;

public class GetCancelledOrdersPagedQueryHandlerTests
{
    [Theory]
    [InlineData(0, 0, 1, 10)]
    [InlineData(-5, -1, 1, 10)]
    [InlineData(2, 500, 2, 100)]
    public async Task Page_and_page_size_are_clamped(
        int requestedPage, int requestedPageSize, int expectedPage, int expectedPageSize)
    {
        var reader = new FakeCancelledOrderReader();
        var handler = new GetCancelledOrdersPagedQueryHandler(reader);

        var result = await handler.Handle(
            new GetCancelledOrdersPagedQuery(requestedPage, requestedPageSize), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(expectedPage, reader.LastPage);
        Assert.Equal(expectedPageSize, reader.LastPageSize);
    }

    [Fact]
    public async Task A_valid_page_returns_the_paged_result()
    {
        var reader = new FakeCancelledOrderReader
        {
            Rows = new[]
            {
                new CancelledOrderDto(1, 10, 7, 3, 5, DateTime.UtcNow),
                new CancelledOrderDto(2, 11, 7, 3, 5, DateTime.UtcNow)
            }
        };
        var handler = new GetCancelledOrdersPagedQueryHandler(reader);

        var result = await handler.Handle(
            new GetCancelledOrdersPagedQuery(1, 10), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value!.TotalCount);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(1, result.Value.TotalPages);
    }
}
