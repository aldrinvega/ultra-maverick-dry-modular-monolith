using Ultramaverick.Orders.Application.Abstractions;
using Ultramaverick.Orders.Application.Queries.CancelledOrders;

namespace Ultramaverick.Orders.UnitTests.Application;

internal sealed class FakeCancelledOrderReader : ICancelledOrderReader
{
    public IReadOnlyList<CancelledOrderDto> Rows { get; set; } = Array.Empty<CancelledOrderDto>();
    public int? LastCustomerId { get; private set; }

    public Task<IReadOnlyList<CancelledOrderDto>> GetByCustomerAsync(int customerId, CancellationToken ct)
    {
        LastCustomerId = customerId;
        return Task.FromResult(Rows);
    }
}

public class GetCancelledOrdersByCustomerQueryHandlerTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task A_non_positive_customer_id_is_a_failure(int customerId)
    {
        var reader = new FakeCancelledOrderReader();
        var handler = new GetCancelledOrdersByCustomerQueryHandler(reader);

        var result = await handler.Handle(
            new GetCancelledOrdersByCustomerQuery(customerId), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Null(reader.LastCustomerId);
    }

    [Fact]
    public async Task A_valid_customer_returns_the_matching_cancelled_orders()
    {
        var reader = new FakeCancelledOrderReader
        {
            Rows = new[]
            {
                new CancelledOrderDto(1, 10, 7, 3, 5, DateTime.UtcNow)
            }
        };
        var handler = new GetCancelledOrdersByCustomerQueryHandler(reader);

        var result = await handler.Handle(
            new GetCancelledOrdersByCustomerQuery(7), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(7, reader.LastCustomerId);
        Assert.Single(result.Value!);
    }
}
