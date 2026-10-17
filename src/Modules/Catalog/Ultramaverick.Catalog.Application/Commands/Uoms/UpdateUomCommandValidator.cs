using FluentValidation;

namespace Ultramaverick.Catalog.Application.Commands.Uoms;

public sealed class UpdateUomCommandValidator : AbstractValidator<UpdateUomCommand>
{
    public UpdateUomCommandValidator()
    {
        RuleFor(x => x.UomId).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(100);
    }
}