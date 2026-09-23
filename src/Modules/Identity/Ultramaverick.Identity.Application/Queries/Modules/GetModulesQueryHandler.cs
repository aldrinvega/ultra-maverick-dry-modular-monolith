using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.Modules
{
    public sealed class GetModulesQueryHandler : IRequestHandler<GetModulesQuery, PagedResult<ModuleDto>>
    {
        private readonly IModuleRepository _modules;

        public GetModulesQueryHandler(IModuleRepository modules) => _modules = modules;

        public async Task<PagedResult<ModuleDto>> Handle(GetModulesQuery request, CancellationToken ct)
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;
            var skip = (page - 1) * pageSize;

            var modules = await _modules.ListAsync(request.Search, request.IsActive, skip, pageSize, ct);
            var total = await _modules.CountAsync(request.Search, request.IsActive, ct);

            var items = modules.Select(m => new ModuleDto(m.Id, m.Name, m.IsActive)).ToList();

            return new PagedResult<ModuleDto>(items, page, pageSize, total);
        }
    }
}
