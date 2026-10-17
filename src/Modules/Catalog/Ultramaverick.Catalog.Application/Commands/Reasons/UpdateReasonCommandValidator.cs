using FluentValidation;

namespace Ultramaverick.Catalog.Application.Commands.Reasons;

public sealed class UpdateReasonCommandValidator : AbstractValidator<UpdateReasonCommand>
{
    public UpdateReasonCommandValidator()
    {
        RuleFor(x => x.ReasonId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.MenuId).GreaterThan(0).When(x => x.MenuId.HasValue);
    }
}