using FluentValidation;

namespace Ultramaverick.Catalog.Application.Commands.Lookups;

public sealed class CreateAccountTitleCommandValidator : AbstractValidator<CreateAccountTitleCommand>
{
    public CreateAccountTitleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).MaximumLength(50);
    }
}