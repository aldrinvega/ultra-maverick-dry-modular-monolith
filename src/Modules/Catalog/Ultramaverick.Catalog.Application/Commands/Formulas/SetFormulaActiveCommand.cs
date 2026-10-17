using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Formulas;

public sealed record SetFormulaActiveCommand(int FormulaId, bool IsActive) : IRequest<Result>;