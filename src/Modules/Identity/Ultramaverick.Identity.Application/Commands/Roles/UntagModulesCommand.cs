using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Roles
{
    /// <summary>Deactivates the listed grants for a role, leaving other grants untouched.</summary>
    public sealed record UntagModulesCommand(int RoleId, IReadOnlyCollection<int>? ModuleIds)
        : IRequest<Result>;
}
