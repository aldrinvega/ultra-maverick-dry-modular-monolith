using Ultramaverick.Orders.Persistence.Repositories;

namespace Ultramaverick.Orders.IntegrationTests;

/// <summary>
/// Proves the daily order-number generator reads the highest existing SO-yyyyMMdd-NNNN number
/// and returns the next one, starting at 0001 for a fresh day.
/// </summary>
public class OrderNumberGeneratorTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public OrderNumberGeneratorTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task NextAsync_starts_at_0001_for_a_new_day_scope()
    {
        await using var context = _fixture.CreateContext();
        await TestData.ClearOrdersAsync(context);

        var generator = new OrderNumberGenerator(context);
        var number = await generator.NextAsync(new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc), CancellationToken.None);

        Assert.Equal("SO-20260102-0001", number);
    }

    [Fact]
    public async Task NextAsync_increments_within_the_same_day_scope()
    {
        await using var context = _fixture.CreateContext();
        await TestData.ClearOrdersAsync(context);

        var utcNow = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc);
        await TestData.CreateOrderAsync(context, "SO-20260103-0009");

        var generator = new OrderNumberGenerator(context);
        var number = await generator.NextAsync(utcNow, CancellationToken.None);

        Assert.Equal("SO-20260103-0010", number);
    }

    [Fact]
    public async Task NextAsync_ranks_by_digit_count_past_9999()
    {
        await using var context = _fixture.CreateContext();
        await TestData.ClearOrdersAsync(context);

        var utcNow = DateTime.UtcNow;
        await TestData.CreateOrderAsync(context, $"SO-{utcNow:yyyyMMdd}-9999");
        await TestData.CreateOrderAsync(context, $"SO-{utcNow:yyyyMMdd}-10000");

        var generator = new OrderNumberGenerator(context);
        var number = await generator.NextAsync(utcNow, CancellationToken.None);

        Assert.Equal($"SO-{utcNow:yyyyMMdd}-10001", number);
    }
}
