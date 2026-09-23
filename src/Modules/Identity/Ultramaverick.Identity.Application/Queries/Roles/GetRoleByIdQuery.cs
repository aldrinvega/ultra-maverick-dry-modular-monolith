using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.Roles
{
    public sealed record GetRoleByIdQuery(int RoleId) : IRequest<Result<RoleDto>>;
}
