using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Modules
{
    public sealed class SetModuleActiveCommandHandler : IRequestHandler<SetModuleActiveCommand, Result>
    {
        private readonly IModuleRepository _modules;
        private readonly IIdentityUnitOfWork _uow;
        private readonly ICurrentUser _currentUser;

        public SetModuleActiveCommandHandler(IModuleRepository modules, IIdentityUnitOfWork uow, ICurrentUser currentUser)
        {
            _modules = modules;
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(SetModuleActiveCommand request, CancellationToken ct)
        {
            var module = await _modules.GetByIdAsync(request.ModuleId, ct);
            if (module is null) return Result.Failure("Module not found.");

            var actorId = _currentUser.UserId
                ?? throw new InvalidOperationException("An authenticated user is required.");

            if (request.IsActive) module.Activate(actorId);
            else module.Deactivate(actorId);

            await _uow.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
