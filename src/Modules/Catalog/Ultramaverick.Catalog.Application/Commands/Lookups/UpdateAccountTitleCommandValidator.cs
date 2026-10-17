using FluentValidation;

namespace Ultramaverick.Catalog.Application.Commands.Lookups;

public sealed class UpdateAccountTitleCommandValidator : AbstractValidator<UpdateAccountTitleCommand>
{
    public UpdateAccountTitleCommandValidator()
    {
        RuleFor(x => x.AccountTitleId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).MaximumLength(50);
    }
}