using FluentValidation;

namespace Ultramaverick.Identity.Application.Commands.MainMenus
{
    public sealed class UpdateMainMenuCommandValidator : AbstractValidator<UpdateMainMenuCommand>
    {
        public UpdateMainMenuCommandValidator()
        {
            RuleFor(x => x.MenuId).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Path).NotEmpty().MaximumLength(300);
            RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        }
    }
}
