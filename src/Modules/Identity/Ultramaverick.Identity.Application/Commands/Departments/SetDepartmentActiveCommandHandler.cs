using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Departments
{
    public sealed class SetDepartmentActiveCommandHandler : IRequestHandler<SetDepartmentActiveCommand, Result>
    {
        private readonly IIdentityUnitOfWork _uow;
        private readonly ICurrentUser _currentUser;

        public SetDepartmentActiveCommandHandler(IIdentityUnitOfWork uow, ICurrentUser currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(SetDepartmentActiveCommand request, CancellationToken ct)
        {
            var department = await _uow.Departments.GetByIdAsync(request.DepartmentId, ct);
            if (department is null) return Result.Failure("Department not found.");

            var actorId = _currentUser.UserId
                ?? throw new InvalidOperationException("An authenticated user is required.");

            if (request.IsActive) department.Activate(actorId);
            else department.Deactivate(actorId);

            await _uow.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
