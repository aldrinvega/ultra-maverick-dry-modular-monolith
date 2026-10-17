using FluentValidation;

namespace Ultramaverick.Catalog.Application.Commands.Reasons;

public sealed class CreateReasonCommandValidator : AbstractValidator<CreateReasonCommand>
{
    public CreateReasonCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.MenuId).GreaterThan(0).When(x => x.MenuId.HasValue);
    }
}