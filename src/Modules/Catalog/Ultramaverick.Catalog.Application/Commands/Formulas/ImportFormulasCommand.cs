using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Formulas;

/// <summary>
/// Imports the legacy transformation (formula) workbook. The stream is read once by
/// <c>IImportReader</c>; transport validation (name, content type, size) belongs to the API layer.
/// </summary>
public sealed record ImportFormulasCommand(Stream Content) : IRequest<Result<ImportReport>>;
