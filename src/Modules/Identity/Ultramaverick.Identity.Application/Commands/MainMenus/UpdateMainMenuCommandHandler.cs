using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.MainMenus
{
    public sealed class UpdateMainMenuCommandHandler : IRequestHandler<UpdateMainMenuCommand, Result>
    {
        private readonly IMainMenuRepository _menus;
        private readonly IIdentityUnitOfWork _uow;
        private readonly ICurrentUser _currentUser;

        public UpdateMainMenuCommandHandler(IMainMenuRepository menus, IIdentityUnitOfWork uow, ICurrentUser currentUser)
        {
            _menus = menus;
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(UpdateMainMenuCommand request, CancellationToken ct)
        {
            var menu = await _menus.GetByIdAsync(request.MenuId, ct);
            if (menu is null) return Result.Failure("Main menu not found.");

            var actorId = _currentUser.UserId
                ?? throw new InvalidOperationException("An authenticated user is required.");

            menu.Update(request.Name, request.Path, request.SortOrder, actorId);
            await _uow.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
