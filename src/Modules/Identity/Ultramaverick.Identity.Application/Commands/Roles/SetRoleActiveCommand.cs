using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Roles
{
    public sealed record SetRoleActiveCommand(int RoleId, bool IsActive) : IRequest<Result>;
}
