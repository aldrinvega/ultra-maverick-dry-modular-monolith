using Ultramaverick.Procurement.Domain.Entities;

namespace Ultramaverick.Procurement.Application.Abstractions;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByNumberAsync(string poNumber, CancellationToken ct);
    Task<IReadOnlyList<PurchaseOrder>> ListAsync(string? search, int skip, int take, CancellationToken ct);
    Task<int> CountAsync(string? search, CancellationToken ct);
    Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken ct);
}
