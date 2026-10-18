using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.UnitTests.Domain;

public class InventoryBalanceTests
{
    [Fact]
    public void Create_starts_at_zero_on_hand()
    {
        var balance = InventoryBalance.Create(1, 10, 20, "LOT-1");

        Assert.Equal(1, balance.WarehouseId);
        Assert.Equal(10, balance.ItemId);
        Assert.Equal(20, balance.UomId);
        Assert.Equal("LOT-1", balance.LotNo);
        Assert.Equal(0m, balance.OnHand);
    }

    [Fact]
    public void Apply_a_positive_delta_increases_on_hand()
    {
        var balance = InventoryBalance.Create(1, 10, 20, null);

        balance.Apply(5m);

        Assert.Equal(5m, balance.OnHand);
    }

    [Fact]
    public void Apply_a_negative_delta_decreases_on_hand()
    {
        var balance = InventoryBalance.Create(1, 10, 20, null);
        balance.Apply(5m);

        balance.Apply(-2m);

        Assert.Equal(3m, balance.OnHand);
    }

    [Fact]
    public void Apply_that_would_go_negative_throws()
    {
        var balance = InventoryBalance.Create(1, 10, 20, null);
        balance.Apply(1m);

        Assert.Throws<InvalidOperationException>(() => balance.Apply(-2m));
    }
}
