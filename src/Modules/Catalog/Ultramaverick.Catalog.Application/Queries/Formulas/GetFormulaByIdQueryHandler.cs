using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Formulas;

public sealed class GetFormulaByIdQueryHandler : IRequestHandler<GetFormulaByIdQuery, Result<FormulaDto>>
{
    private readonly ICatalogUnitOfWork _uow;

    public GetFormulaByIdQueryHandler(ICatalogUnitOfWork uow) => _uow = uow;

    public async Task<Result<FormulaDto>> Handle(GetFormulaByIdQuery request, CancellationToken ct)
    {
        var formula = await _uow.Formulas.GetByIdAsync(request.FormulaId, ct);
        if (formula is null) return Result<FormulaDto>.Failure("Formula not found.");

        return Result<FormulaDto>.Success(GetFormulasQueryHandler.Map(formula));
    }
}