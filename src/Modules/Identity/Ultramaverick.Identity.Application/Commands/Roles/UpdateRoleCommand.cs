using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Roles
{
    public sealed record UpdateRoleCommand(int RoleId, string Name) : IRequest<Result>;
}
