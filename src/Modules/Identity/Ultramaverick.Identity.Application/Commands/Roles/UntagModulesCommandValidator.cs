using FluentValidation;

namespace Ultramaverick.Identity.Application.Commands.Roles
{
    public sealed class UntagModulesCommandValidator : AbstractValidator<UntagModulesCommand>
    {
        public UntagModulesCommandValidator()
        {
            RuleFor(x => x.RoleId).GreaterThan(0);
            RuleFor(x => x.ModuleIds).NotEmpty();
        }
    }
}
