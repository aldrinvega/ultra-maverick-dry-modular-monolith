using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Formulas;

public sealed record AddFormulaRequirementCommand(int FormulaId, int RawMaterialId, decimal Quantity)
    : IRequest<Result>;