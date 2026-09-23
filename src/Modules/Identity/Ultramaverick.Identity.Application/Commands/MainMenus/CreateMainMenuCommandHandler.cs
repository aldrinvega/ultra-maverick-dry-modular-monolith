using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;
using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Application.Commands.MainMenus
{
    public sealed class CreateMainMenuCommandHandler : IRequestHandler<CreateMainMenuCommand, Result<int>>
    {
        private readonly IMainMenuRepository _menus;
        private readonly IIdentityUnitOfWork _uow;

        public CreateMainMenuCommandHandler(IMainMenuRepository menus, IIdentityUnitOfWork uow)
        {
            _menus = menus;
            _uow = uow;
        }

        public async Task<Result<int>> Handle(CreateMainMenuCommand request, CancellationToken ct)
        {
            var menu = MainMenu.Create(request.Name, request.Path, request.SortOrder);

            await _menus.AddAsync(menu, ct);
            await _uow.SaveChangesAsync(ct);   // shares the scoped DbContext

            return Result<int>.Success(menu.Id);
        }
    }
}
