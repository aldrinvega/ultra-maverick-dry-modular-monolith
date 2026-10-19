using Ultramaverick.Quality.Application.Queries.QcReceipts;

namespace Ultramaverick.Quality.Application.Abstractions;

public interface IQcReceiptReader
{
    Task<QcReceiptDetailDto?> GetByIdAsync(int qcReceiptId, CancellationToken ct);
}
