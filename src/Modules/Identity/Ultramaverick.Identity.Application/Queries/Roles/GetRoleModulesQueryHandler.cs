using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.Roles
{
    public sealed class GetRoleModulesQueryHandler : IRequestHandler<GetRoleModulesQuery, Result<IReadOnlyList<ModuleDto>>>
    {
        private readonly IRoleRepository _roles;
        private readonly IModuleRepository _modules;

        public GetRoleModulesQueryHandler(IRoleRepository roles, IModuleRepository modules)
        {
            _roles = roles;
            _modules = modules;
        }

        public async Task<Result<IReadOnlyList<ModuleDto>>> Handle(GetRoleModulesQuery request, CancellationToken ct)
        {
            if (await _roles.GetByIdAsync(request.RoleId, ct) is null)
                return Result<IReadOnlyList<ModuleDto>>.Failure("Role not found.");

            var modules = await _modules.GetByRoleIdAsync(request.RoleId, ct);

            IReadOnlyList<ModuleDto> items = modules
                .Select(m => new ModuleDto(m.Id, m.Name, m.IsActive))
                .ToList();

            return Result<IReadOnlyList<ModuleDto>>.Success(items);
        }
    }
}
