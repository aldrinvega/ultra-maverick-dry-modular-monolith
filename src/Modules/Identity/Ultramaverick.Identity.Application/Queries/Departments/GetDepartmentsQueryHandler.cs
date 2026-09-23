using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.Departments
{
    public sealed class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, PagedResult<DepartmentDto>>
    {
        private readonly IIdentityUnitOfWork _uow;

        public GetDepartmentsQueryHandler(IIdentityUnitOfWork uow) => _uow = uow;

        public async Task<PagedResult<DepartmentDto>> Handle(GetDepartmentsQuery request, CancellationToken ct)
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;
            var skip = (page - 1) * pageSize;

            var departments = await _uow.Departments.ListAsync(request.Search, request.IsActive, skip, pageSize, ct);
            var total = await _uow.Departments.CountAsync(request.Search, request.IsActive, ct);

            var items = departments
                .Select(d => new DepartmentDto(d.Id, d.Name, d.IsActive))
                .ToList();

            return new PagedResult<DepartmentDto>(items, page, pageSize, total);
        }
    }
}
