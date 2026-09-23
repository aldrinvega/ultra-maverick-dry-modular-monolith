using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.Roles
{
    public sealed class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, PagedResult<RoleDto>>
    {
        private readonly IIdentityUnitOfWork _uow;

        public GetRolesQueryHandler(IIdentityUnitOfWork uow) => _uow = uow;

        public async Task<PagedResult<RoleDto>> Handle(GetRolesQuery request, CancellationToken ct)
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;
            var skip = (page - 1) * pageSize;

            var roles = await _uow.Roles.ListAsync(request.Search, request.IsActive, skip, pageSize, ct);
            var total = await _uow.Roles.CountAsync(request.Search, request.IsActive, ct);

            var items = roles.Select(r => new RoleDto(r.Id, r.Name, r.IsActive)).ToList();

            return new PagedResult<RoleDto>(items, page, pageSize, total);
        }
    }
}
