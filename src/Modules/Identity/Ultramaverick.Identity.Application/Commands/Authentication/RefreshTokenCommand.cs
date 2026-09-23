using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Authentication
{
    public sealed record RefreshTokenCommand(string RefreshToken)
        : IRequest<Result<AuthenticateResponse>>;
}
