using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Formulas;

public sealed record CreateFormulaCommand(int ItemId, int Version, decimal Quantity) : IRequest<Result<int>>;