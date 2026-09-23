using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Roles
{
    public sealed class TagModulesCommandHandler : IRequestHandler<TagModulesCommand, Result>
    {
        private readonly IIdentityUnitOfWork _uow;
        private readonly IModuleRepository _modules;
        private readonly ICurrentUser _currentUser;

        public TagModulesCommandHandler(
            IIdentityUnitOfWork uow, IModuleRepository modules, ICurrentUser currentUser)
        {
            _uow = uow;
            _modules = modules;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(TagModulesCommand request, CancellationToken ct)
        {
            if (await _uow.Roles.GetByIdAsync(request.RoleId, ct) is null)
                return Result.Failure("Role not found.");

            var requested = request.ModuleIds?.Distinct().ToArray() ?? Array.Empty<int>();

            if (requested.Length > 0)
            {
                var existing = await _modules.GetExistingIdsAsync(requested, ct);
                var missing = requested.Except(existing).ToArray();

                if (missing.Length > 0)
                    return Result.Failure($"Unknown module id(s): {string.Join(", ", missing)}.");
            }

            var actorId = _currentUser.UserId
                ?? throw new InvalidOperationException("An authenticated user is required.");

            await _uow.Roles.SetModulesAsync(request.RoleId, requested, actorId, ct);
            await _uow.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
