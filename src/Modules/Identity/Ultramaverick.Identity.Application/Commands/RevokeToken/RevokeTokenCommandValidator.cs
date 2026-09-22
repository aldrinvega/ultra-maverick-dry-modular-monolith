using FluentValidation;

namespace Ultramaverick.Identity.Application.Commands.RevokeToken
{
    public sealed class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
    {
        public RevokeTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(512);
        }
    }
}
