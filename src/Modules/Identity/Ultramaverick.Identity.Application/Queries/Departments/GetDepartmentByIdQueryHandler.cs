using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.Departments
{
    public sealed class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, Result<DepartmentDto>>
    {
        private readonly IIdentityUnitOfWork _uow;

        public GetDepartmentByIdQueryHandler(IIdentityUnitOfWork uow) => _uow = uow;

        public async Task<Result<DepartmentDto>> Handle(GetDepartmentByIdQuery request, CancellationToken ct)
        {
            var department = await _uow.Departments.GetByIdAsync(request.DepartmentId, ct);
            if (department is null) return Result<DepartmentDto>.Failure("Department not found.");

            return Result<DepartmentDto>.Success(new DepartmentDto(department.Id, department.Name, department.IsActive));
        }
    }
}
