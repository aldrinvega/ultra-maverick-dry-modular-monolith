using FluentValidation;

namespace Ultramaverick.Catalog.Application.Commands.Formulas;

public sealed class CreateFormulaCommandValidator : AbstractValidator<CreateFormulaCommand>
{
    public CreateFormulaCommandValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}