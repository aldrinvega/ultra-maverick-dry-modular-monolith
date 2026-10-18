using Ultramaverick.Quality.Domain.Entities;

namespace Ultramaverick.Quality.Application.Abstractions;

public interface IQcReceiptRepository
{
    Task<bool> ExistsForPurchaseOrderAsync(int purchaseOrderId, CancellationToken ct);
    Task<QcReceipt?> GetByIdAsync(int qcReceiptId, CancellationToken ct);
    Task<IReadOnlyList<QcReceipt>> ListAsync(string? search, int skip, int take, CancellationToken ct);
    Task<int> CountAsync(string? search, CancellationToken ct);
    Task AddAsync(QcReceipt receipt, CancellationToken ct);
}
