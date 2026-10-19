using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Items;

/// <summary>
/// Imports the legacy raw-material setup workbook. The stream is read once by
/// <c>IImportReader</c>; transport validation (name, content type, size) belongs to the API layer.
/// </summary>
public sealed record ImportRawMaterialsCommand(Stream Content) : IRequest<Result<ImportReport>>;
