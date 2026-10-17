using FluentValidation;

namespace Ultramaverick.Catalog.Application.Commands.LotNames;

public sealed class UpdateLotNameCommandValidator : AbstractValidator<UpdateLotNameCommand>
{
    public UpdateLotNameCommandValidator()
    {
        RuleFor(x => x.LotNameId).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LotCategoryId).GreaterThan(0);
        RuleFor(x => x.SectionName).MaximumLength(100);
    }
}