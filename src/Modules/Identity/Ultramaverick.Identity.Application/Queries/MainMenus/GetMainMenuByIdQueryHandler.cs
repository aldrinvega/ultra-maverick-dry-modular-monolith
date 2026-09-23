using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.MainMenus
{
    public sealed class GetMainMenuByIdQueryHandler : IRequestHandler<GetMainMenuByIdQuery, Result<MainMenuDto>>
    {
        private readonly IMainMenuRepository _menus;

        public GetMainMenuByIdQueryHandler(IMainMenuRepository menus) => _menus = menus;

        public async Task<Result<MainMenuDto>> Handle(GetMainMenuByIdQuery request, CancellationToken ct)
        {
            var menu = await _menus.GetByIdAsync(request.MenuId, ct);
            if (menu is null) return Result<MainMenuDto>.Failure("Main menu not found.");

            return Result<MainMenuDto>.Success(
                new MainMenuDto(menu.Id, menu.Name, menu.Path, menu.SortOrder, menu.IsActive));
        }
    }
}
