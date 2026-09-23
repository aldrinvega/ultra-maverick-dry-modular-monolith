using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Roles
{
    public sealed class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result>
    {
        private readonly IIdentityUnitOfWork _uow;
        private readonly ICurrentUser _currentUser;

        public UpdateRoleCommandHandler(IIdentityUnitOfWork uow, ICurrentUser currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken ct)
        {
            var role = await _uow.Roles.GetByIdAsync(request.RoleId, ct);
            if (role is null) return Result.Failure("Role not found.");

            var name = request.Name.Trim();
            var existing = await _uow.Roles.GetByNameAsync(name, ct);
            if (existing is not null && existing.Id != role.Id)
                return Result.Failure($"Role '{name}' already exists.");

            var actorId = _currentUser.UserId
                ?? throw new InvalidOperationException("An authenticated user is required.");

            role.Rename(name, actorId);
            await _uow.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
