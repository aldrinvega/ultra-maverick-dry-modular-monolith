using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;
using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Application.Commands.Departments
{
    public sealed class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Result<int>>
    {
        private readonly IIdentityUnitOfWork _uow;

        public CreateDepartmentCommandHandler(IIdentityUnitOfWork uow) => _uow = uow;

        public async Task<Result<int>> Handle(CreateDepartmentCommand request, CancellationToken ct)
        {
            var name = request.Name.Trim();

            if (await _uow.Departments.GetByNameAsync(name, ct) is not null)
                return Result<int>.Failure($"Department '{name}' already exists.");

            var department = Department.Create(name);

            await _uow.Departments.AddAsync(department, ct);
            await _uow.SaveChangesAsync(ct);

            return Result<int>.Success(department.Id);
        }
    }
}
