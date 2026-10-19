using Ultramaverick.Orders.Application.Abstractions;
using Ultramaverick.Orders.Application.Models;
using Ultramaverick.Orders.Application.Queries.Orders;

namespace Ultramaverick.Orders.UnitTests.Application;

internal sealed class FakeOrderReader : IOrderReader
{
    public PagedResult<OrderSummaryDto> Orders { get; set; } =
        new(Array.Empty<OrderSummaryDto>(), 1, 25, 0);

    public OrderDetailDto? Detail { get; set; }

    public int? LastPage { get; private set; }
    public int? LastPageSize { get; private set; }
    public int? LastCustomerId { get; private set; }
    public string? LastStatus { get; private set; }

    public Task<PagedResult<OrderSummaryDto>> GetPagedAsync(
        int? customerId, string? status, int page, int pageSize, CancellationToken ct)
    {
        LastCustomerId = customerId;
        LastStatus = status;
        LastPage = page;
        LastPageSize = pageSize;
        return Task.FromResult(Orders);
    }

    public Task<OrderDetailDto?> GetByIdAsync(int orderId, CancellationToken ct)
        => Task.FromResult(Detail);

    public Task<PagedResult<MoveOrderDto>> GetMoveOrdersPagedAsync(
        int page, int pageSize, string? status, CancellationToken ct)
    {
        LastPage = page;
        LastPageSize = pageSize;
        LastStatus = status;
        return Task.FromResult(new PagedResult<MoveOrderDto>(Array.Empty<MoveOrderDto>(), page, pageSize, 0));
    }

    public Task<PagedResult<TransactedMoveOrderDto>> GetTransactedPagedAsync(
        int page, int pageSize, CancellationToken ct)
    {
        LastPage = page;
        LastPageSize = pageSize;
        return Task.FromResult(
            new PagedResult<TransactedMoveOrderDto>(Array.Empty<TransactedMoveOrderDto>(), page, pageSize, 0));
    }
}

public class OrderQueryHandlerTests
{
    [Theory]
    [InlineData(0, 0, 1, 25)]
    [InlineData(-3, 500, 1, 100)]
    [InlineData(4, 10, 4, 10)]
    public async Task Listing_orders_clamps_the_page_window(
        int requestedPage, int requestedSize, int expectedPage, int expectedSize)
    {
        var reader = new FakeOrderReader();
        var handler = new GetOrdersQueryHandler(reader);

        var result = await handler.Handle(
            new GetOrdersQuery(null, null, requestedPage, requestedSize), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(expectedPage, reader.LastPage);
        Assert.Equal(expectedSize, reader.LastPageSize);
    }

    [Fact]
    public async Task Listing_orders_passes_the_filters_through()
    {
        var reader = new FakeOrderReader();
        var handler = new GetOrdersQueryHandler(reader);

        await handler.Handle(new GetOrdersQuery(42, "Confirmed", 1, 25), CancellationToken.None);

        Assert.Equal(42, reader.LastCustomerId);
        Assert.Equal("Confirmed", reader.LastStatus);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task An_invalid_order_id_is_a_failure(int orderId)
    {
        var reader = new FakeOrderReader();
        var handler = new GetOrderByIdQueryHandler(reader);

        var result = await handler.Handle(new GetOrderByIdQuery(orderId), CancellationToken.None);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task A_missing_order_is_a_failure()
    {
        var reader = new FakeOrderReader { Detail = null };
        var handler = new GetOrderByIdQueryHandler(reader);

        var result = await handler.Handle(new GetOrderByIdQuery(7), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Order not found.", result.Error);
    }

    [Fact]
    public async Task An_existing_order_is_returned_with_its_lines()
    {
        var reader = new FakeOrderReader
        {
            Detail = new OrderDetailDto(
                7, "SO-7", 3, null, null, null, null, "Draft", null, true,
                new[] { new OrderLineDto(1, 10, 20, 5m, 0m, null, null, true) })
        };
        var handler = new GetOrderByIdQueryHandler(reader);

        var result = await handler.Handle(new GetOrderByIdQuery(7), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(result.Value!.Lines);
    }
}
