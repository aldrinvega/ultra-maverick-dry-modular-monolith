using FluentValidation;

namespace Ultramaverick.Warehouse.Application.Commands.Inventory;

public sealed class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.UomId).GreaterThan(0);
        RuleFor(x => x.Delta).NotEqual(0m).PrecisionScale(18, 4, ignoreTrailingZeros: false);
    }
}
