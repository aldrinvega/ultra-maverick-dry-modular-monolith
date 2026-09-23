using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Roles
{
    public sealed class SetRoleActiveCommandHandler : IRequestHandler<SetRoleActiveCommand, Result>
    {
        private readonly IIdentityUnitOfWork _uow;
        private readonly ICurrentUser _currentUser;

        public SetRoleActiveCommandHandler(IIdentityUnitOfWork uow, ICurrentUser currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(SetRoleActiveCommand request, CancellationToken ct)
        {
            var role = await _uow.Roles.GetByIdAsync(request.RoleId, ct);
            if (role is null) return Result.Failure("Role not found.");

            var actorId = _currentUser.UserId
                ?? throw new InvalidOperationException("An authenticated user is required.");

            if (request.IsActive) role.Activate(actorId);
            else role.Deactivate(actorId);

            await _uow.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
