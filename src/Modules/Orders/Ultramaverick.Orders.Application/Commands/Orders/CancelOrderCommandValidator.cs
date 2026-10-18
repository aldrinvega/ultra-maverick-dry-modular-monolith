using FluentValidation;

namespace Ultramaverick.Orders.Application.Commands.Orders;

public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.Reason).MaximumLength(1000);
    }
}
