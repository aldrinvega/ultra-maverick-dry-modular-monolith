using FluentValidation;

namespace Ultramaverick.Catalog.Application.Commands.LotNames;

public sealed class CreateLotNameCommandValidator : AbstractValidator<CreateLotNameCommand>
{
    public CreateLotNameCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LotCategoryId).GreaterThan(0);
        RuleFor(x => x.SectionName).MaximumLength(100);
    }
}