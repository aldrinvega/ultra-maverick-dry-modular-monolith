using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Roles
{
    public sealed class UntagModulesCommandHandler : IRequestHandler<UntagModulesCommand, Result>
    {
        private readonly IIdentityUnitOfWork _uow;
        private readonly ICurrentUser _currentUser;

        public UntagModulesCommandHandler(IIdentityUnitOfWork uow, ICurrentUser currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(UntagModulesCommand request, CancellationToken ct)
        {
            if (await _uow.Roles.GetByIdAsync(request.RoleId, ct) is null)
                return Result.Failure("Role not found.");

            var requested = request.ModuleIds?.Distinct().ToArray() ?? Array.Empty<int>();
            if (requested.Length == 0)
                return Result.Failure("No module ids were supplied.");

            var actorId = _currentUser.UserId
                ?? throw new InvalidOperationException("An authenticated user is required.");

            await _uow.Roles.DeactivateModulesAsync(request.RoleId, requested, actorId, ct);
            await _uow.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
