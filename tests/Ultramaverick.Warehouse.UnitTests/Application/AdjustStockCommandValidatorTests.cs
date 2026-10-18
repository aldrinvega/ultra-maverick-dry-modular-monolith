using Ultramaverick.Warehouse.Application.Commands.Inventory;

namespace Ultramaverick.Warehouse.UnitTests.Application;

public class AdjustStockCommandValidatorTests
{
    private readonly AdjustStockCommandValidator _validator = new();

    [Fact]
    public void A_non_zero_delta_is_valid()
        => Assert.True(_validator.Validate(
            new AdjustStockCommand(1, 10, 20, null, -2m)).IsValid);

    [Fact]
    public void A_zero_delta_is_invalid()
        => Assert.False(_validator.Validate(
            new AdjustStockCommand(1, 10, 20, null, 0m)).IsValid);

    [Fact]
    public void A_non_positive_warehouse_id_is_invalid()
        => Assert.False(_validator.Validate(
            new AdjustStockCommand(0, 10, 20, null, 1m)).IsValid);
}
