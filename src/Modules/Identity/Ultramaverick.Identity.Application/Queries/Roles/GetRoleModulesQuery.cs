using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.Roles
{
    public sealed record GetRoleModulesQuery(int RoleId) : IRequest<Result<IReadOnlyList<ModuleDto>>>;
}
