using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.RevokeToken
{
    public sealed record RevokeTokenCommand(string RefreshToken) : IRequest<Result>;
}
