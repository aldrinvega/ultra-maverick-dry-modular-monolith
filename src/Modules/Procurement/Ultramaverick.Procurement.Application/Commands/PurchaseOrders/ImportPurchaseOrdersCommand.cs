using MediatR;
using Ultramaverick.Procurement.Application.Models;

namespace Ultramaverick.Procurement.Application.Commands.PurchaseOrders;

/// <summary>
/// Imports the legacy PO-summary workbook. The stream is read once by <c>IImportReader</c>;
/// transport validation (name, content type, size) belongs to the API layer.
/// </summary>
public sealed record ImportPurchaseOrdersCommand(Stream Content) : IRequest<Result<ImportReport>>;
