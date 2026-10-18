using FluentValidation;

namespace Ultramaverick.Warehouse.Application.Commands.WarehouseReceipts;

public sealed class CreateWarehouseReceiptCommandValidator : AbstractValidator<CreateWarehouseReceiptCommand>
{
    public CreateWarehouseReceiptCommandValidator()
    {
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.UomId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0).PrecisionScale(18, 4, ignoreTrailingZeros: false);
    }
}
