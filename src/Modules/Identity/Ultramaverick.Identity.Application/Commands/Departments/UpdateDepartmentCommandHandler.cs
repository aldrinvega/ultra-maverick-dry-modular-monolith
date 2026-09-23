using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Departments
{
    public sealed class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, Result>
    {
        private readonly IIdentityUnitOfWork _uow;
        private readonly ICurrentUser _currentUser;

        public UpdateDepartmentCommandHandler(IIdentityUnitOfWork uow, ICurrentUser currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(UpdateDepartmentCommand request, CancellationToken ct)
        {
            var department = await _uow.Departments.GetByIdAsync(request.DepartmentId, ct);
            if (department is null) return Result.Failure("Department not found.");

            var name = request.Name.Trim();
            var existing = await _uow.Departments.GetByNameAsync(name, ct);
            if (existing is not null && existing.Id != department.Id)
                return Result.Failure($"Department '{name}' already exists.");

            var actorId = _currentUser.UserId
                ?? throw new InvalidOperationException("An authenticated user is required.");

            department.Rename(name, actorId);
            await _uow.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
