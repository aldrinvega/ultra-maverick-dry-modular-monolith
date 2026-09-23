using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Roles
{
    /// <summary>
    /// Replaces a role's active module grants with the supplied set.
    /// A null or empty set clears every grant for the role.
    /// </summary>
    public sealed record TagModulesCommand(int RoleId, IReadOnlyCollection<int>? ModuleIds)
        : IRequest<Result>;
}
