using Ultramaverick.Reporting.Domain.Entities;

namespace Ultramaverick.Reporting.Application.Abstractions;

public interface IQcStatusStore
{
    Task<QcStatus?> FindAsync(int qcReceiptId, CancellationToken ct);
    Task AddAsync(QcStatus status, CancellationToken ct);
}
