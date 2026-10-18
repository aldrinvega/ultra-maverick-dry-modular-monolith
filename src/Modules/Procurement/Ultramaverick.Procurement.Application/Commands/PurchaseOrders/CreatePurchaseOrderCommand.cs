using MediatR;
using Ultramaverick.Procurement.Application.Models;

namespace Ultramaverick.Procurement.Application.Commands.PurchaseOrders;

public sealed record PurchaseOrderLineInput(int ItemId, int UomId, decimal Quantity, decimal UnitPrice);

public sealed record CreatePurchaseOrderCommand(
    string PoNumber, string? PrNumber, DateTime? PoDate, DateTime? PrDate,
    int SupplierId, IReadOnlyList<PurchaseOrderLineInput> Lines) : IRequest<Result<int>>;
