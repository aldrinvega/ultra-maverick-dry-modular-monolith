using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.Departments
{
    public sealed record GetDepartmentByIdQuery(int DepartmentId) : IRequest<Result<DepartmentDto>>;
}
