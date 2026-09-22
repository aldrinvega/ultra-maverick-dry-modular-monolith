using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries
{
    public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
    {
        private readonly IIdentityUnitOfWork _uow;

        public GetUsersQueryHandler(IIdentityUnitOfWork uow) => _uow = uow;

        public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken ct)
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;
            var skip = (page - 1) * pageSize;

            var users = await _uow.Users.ListAsync(request.Search, request.IsActive, skip, pageSize, ct);
            var total = await _uow.Users.CountAsync(request.Search, request.IsActive, ct);

            var roles = (await _uow.Roles.GetAllAsync(ct)).ToDictionary(r => r.Id, r => r.Name);
            var departments = (await _uow.Departments.GetAllAsync(ct)).ToDictionary(d => d.Id, d => d.Name);

            var items = users.Select(u => new UserDto(
                u.Id,
                u.FullName,
                u.UserName,
                u.RoleId,
                roles.GetValueOrDefault(u.RoleId, string.Empty),
                u.DepartmentId,
                departments.GetValueOrDefault(u.DepartmentId, string.Empty),
                u.IsActive)).ToList();

            return new PagedResult<UserDto>(items, page, pageSize, total);
        }
    }
}
