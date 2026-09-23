using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Roles
{
    public sealed record CreateRoleCommand(string Name) : IRequest<Result<int>>;
}
