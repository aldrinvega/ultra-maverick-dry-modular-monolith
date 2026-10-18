using FluentValidation;

namespace Ultramaverick.Procurement.Application.Commands.PurchaseOrders;

public sealed class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    private const int MaxLines = 500;

    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.PoNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PrNumber).MaximumLength(50);
        RuleFor(x => x.SupplierId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty()
            .Must(lines => lines is null || lines.Count <= MaxLines)
            .WithMessage($"A purchase order cannot contain more than {MaxLines} lines.");
        RuleForEach(x => x.Lines).NotNull().ChildRules(line =>
        {
            line.RuleFor(l => l.ItemId).GreaterThan(0);
            line.RuleFor(l => l.UomId).GreaterThan(0);
            line.RuleFor(l => l.Quantity).GreaterThan(0).PrecisionScale(18, 4, ignoreTrailingZeros: false);
            line.RuleFor(l => l.UnitPrice).PrecisionScale(18, 4, ignoreTrailingZeros: false);
        });
    }
}
