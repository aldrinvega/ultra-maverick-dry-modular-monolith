using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Formulas;

public sealed record GetFormulasQuery(int Page, int PageSize, int? ItemId, bool? IsActive)
    : IRequest<PagedResult<FormulaDto>>;