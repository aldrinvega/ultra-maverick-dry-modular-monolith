using FluentValidation;

namespace Ultramaverick.Identity.Application.Commands.Modules
{
    public sealed class CreateModuleCommandValidator : AbstractValidator<CreateModuleCommand>
    {
        public CreateModuleCommandValidator()
        {
            RuleFor(x => x.MainMenuId).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.SubMenuName).MaximumLength(150);
        }
    }
}
