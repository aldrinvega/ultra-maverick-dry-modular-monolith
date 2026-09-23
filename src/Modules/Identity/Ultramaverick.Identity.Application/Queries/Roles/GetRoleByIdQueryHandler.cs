using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.Roles
{
    public sealed class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
    {
        private readonly IIdentityUnitOfWork _uow;

        public GetRoleByIdQueryHandler(IIdentityUnitOfWork uow) => _uow = uow;

        public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken ct)
        {
            var role = await _uow.Roles.GetByIdAsync(request.RoleId, ct);
            if (role is null) return Result<RoleDto>.Failure("Role not found.");

            return Result<RoleDto>.Success(new RoleDto(role.Id, role.Name, role.IsActive));
        }
    }
}
