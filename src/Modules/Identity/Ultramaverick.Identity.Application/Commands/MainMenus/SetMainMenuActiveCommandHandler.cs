using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.MainMenus
{
    public sealed class SetMainMenuActiveCommandHandler : IRequestHandler<SetMainMenuActiveCommand, Result>
    {
        private readonly IMainMenuRepository _menus;
        private readonly IIdentityUnitOfWork _uow;
        private readonly ICurrentUser _currentUser;

        public SetMainMenuActiveCommandHandler(IMainMenuRepository menus, IIdentityUnitOfWork uow, ICurrentUser currentUser)
        {
            _menus = menus;
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(SetMainMenuActiveCommand request, CancellationToken ct)
        {
            var menu = await _menus.GetByIdAsync(request.MenuId, ct);
            if (menu is null) return Result.Failure("Main menu not found.");

            var actorId = _currentUser.UserId
                ?? throw new InvalidOperationException("An authenticated user is required.");

            if (request.IsActive) menu.Activate(actorId);
            else menu.Deactivate(actorId);

            await _uow.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
