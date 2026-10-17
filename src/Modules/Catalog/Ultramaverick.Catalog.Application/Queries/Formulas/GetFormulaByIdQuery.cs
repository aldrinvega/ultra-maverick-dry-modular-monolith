using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Formulas;

public sealed record GetFormulaByIdQuery(int FormulaId) : IRequest<Result<FormulaDto>>;