using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.RefreshToken
{
    public sealed record RefreshTokenCommand(string RefreshToken)
        : IRequest<Result<AuthenticateResponse>>;
}
