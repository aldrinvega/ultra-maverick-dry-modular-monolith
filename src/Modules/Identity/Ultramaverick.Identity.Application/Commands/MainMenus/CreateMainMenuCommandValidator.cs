using FluentValidation;

namespace Ultramaverick.Identity.Application.Commands.MainMenus
{
    public sealed class CreateMainMenuCommandValidator : AbstractValidator<CreateMainMenuCommand>
    {
        public CreateMainMenuCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Path).NotEmpty().MaximumLength(300);
            RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        }
    }
}
