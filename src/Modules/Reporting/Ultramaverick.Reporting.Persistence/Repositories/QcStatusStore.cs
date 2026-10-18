using Microsoft.EntityFrameworkCore;
using Ultramaverick.Reporting.Application.Abstractions;
using Ultramaverick.Reporting.Domain.Entities;

namespace Ultramaverick.Reporting.Persistence.Repositories;

public sealed class QcStatusStore : IQcStatusStore
{
    private readonly ReportingDbContext _context;

    public QcStatusStore(ReportingDbContext context) => _context = context;

    public Task<QcStatus?> FindAsync(int qcReceiptId, CancellationToken ct)
        => _context.QcStatus.FirstOrDefaultAsync(x => x.QcReceiptId == qcReceiptId, ct);

    public async Task AddAsync(QcStatus status, CancellationToken ct)
        => await _context.QcStatus.AddAsync(status, ct);
}
