using FluentValidation;

namespace Ultramaverick.Identity.Application.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(512);
        }
    }
}
