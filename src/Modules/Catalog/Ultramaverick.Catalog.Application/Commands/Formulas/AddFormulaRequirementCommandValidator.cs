using FluentValidation;

namespace Ultramaverick.Catalog.Application.Commands.Formulas;

public sealed class AddFormulaRequirementCommandValidator : AbstractValidator<AddFormulaRequirementCommand>
{
    public AddFormulaRequirementCommandValidator()
    {
        RuleFor(x => x.FormulaId).GreaterThan(0);
        RuleFor(x => x.RawMaterialId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}