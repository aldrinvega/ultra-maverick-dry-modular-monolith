using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.Departments
{
    public sealed record GetDepartmentsQuery(int Page, int PageSize, string? Search, bool? IsActive)
        : IRequest<PagedResult<DepartmentDto>>;
}
