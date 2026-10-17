using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Queries.Formulas;

public sealed class GetFormulasQueryHandler : IRequestHandler<GetFormulasQuery, PagedResult<FormulaDto>>
{
    private readonly ICatalogUnitOfWork _uow;

    public GetFormulasQueryHandler(ICatalogUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<FormulaDto>> Handle(GetFormulasQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;
        var skip = (page - 1) * pageSize;

        var rows = await _uow.Formulas.ListAsync(request.ItemId, request.IsActive, skip, pageSize, ct);
        var total = await _uow.Formulas.CountAsync(request.ItemId, request.IsActive, ct);

        var items = rows.Select(Map).ToList();

        return new PagedResult<FormulaDto>(items, page, pageSize, total);
    }

    internal static FormulaDto Map(FormulaHeader formula) => new(
        formula.Id,
        formula.ItemId,
        formula.Version,
        formula.Quantity,
        formula.IsActive,
        formula.Requirements
            .Select(r => new FormulaRequirementDto(r.Id, r.RawMaterialId, r.Quantity))
            .ToList());
}