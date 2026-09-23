using FluentValidation;

namespace Ultramaverick.Identity.Application.Commands.Roles
{
    public sealed class TagModulesCommandValidator : AbstractValidator<TagModulesCommand>
    {
        public TagModulesCommandValidator()
        {
            RuleFor(x => x.RoleId).GreaterThan(0);
            // ModuleIds may be null or empty: that clears the role's grants.
        }
    }
}
