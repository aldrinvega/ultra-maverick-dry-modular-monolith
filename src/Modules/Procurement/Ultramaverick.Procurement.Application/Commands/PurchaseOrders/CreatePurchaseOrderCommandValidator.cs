using FluentValidation;

namespace Ultramaverick.Procurement.Application.Commands.PurchaseOrders;

public sealed class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.PoNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PrNumber).MaximumLength(50);
        RuleFor(x => x.SupplierId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ItemId).GreaterThan(0);
            line.RuleFor(l => l.UomId).GreaterThan(0);
            line.RuleFor(l => l.Quantity).GreaterThan(0);
        });
    }
}
