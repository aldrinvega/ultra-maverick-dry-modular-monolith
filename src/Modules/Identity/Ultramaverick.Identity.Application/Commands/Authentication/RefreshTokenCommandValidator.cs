using FluentValidation;

namespace Ultramaverick.Identity.Application.Commands.Authentication
{
    public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(512);
        }
    }
}
