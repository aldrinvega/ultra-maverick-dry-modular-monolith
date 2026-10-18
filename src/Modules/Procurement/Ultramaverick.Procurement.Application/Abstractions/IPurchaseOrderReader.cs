using Ultramaverick.Procurement.Application.Queries.PurchaseOrders;

namespace Ultramaverick.Procurement.Application.Abstractions;

public interface IPurchaseOrderReader
{
    Task<PurchaseOrderDetailDto?> GetByIdAsync(int purchaseOrderId, CancellationToken ct);
}
