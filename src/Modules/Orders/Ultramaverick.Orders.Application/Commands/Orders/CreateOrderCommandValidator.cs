using FluentValidation;

namespace Ultramaverick.Orders.Application.Commands.Orders;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ItemId).GreaterThan(0);
            line.RuleFor(l => l.UomId).GreaterThan(0);
            line.RuleFor(l => l.Quantity).GreaterThan(0).PrecisionScale(18, 4, ignoreTrailingZeros: false);
        });
    }
}
