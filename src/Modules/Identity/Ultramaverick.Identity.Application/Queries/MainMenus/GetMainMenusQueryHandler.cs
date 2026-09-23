using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.MainMenus
{
    public sealed class GetMainMenusQueryHandler : IRequestHandler<GetMainMenusQuery, PagedResult<MainMenuDto>>
    {
        private readonly IMainMenuRepository _menus;

        public GetMainMenusQueryHandler(IMainMenuRepository menus) => _menus = menus;

        public async Task<PagedResult<MainMenuDto>> Handle(GetMainMenusQuery request, CancellationToken ct)
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;
            var skip = (page - 1) * pageSize;

            var menus = await _menus.ListAsync(request.Search, request.IsActive, skip, pageSize, ct);
            var total = await _menus.CountAsync(request.Search, request.IsActive, ct);

            var items = menus
                .Select(m => new MainMenuDto(m.Id, m.Name, m.Path, m.SortOrder, m.IsActive))
                .ToList();

            return new PagedResult<MainMenuDto>(items, page, pageSize, total);
        }
    }
}
