using Ultramaverick.Quality.Application.Abstractions;

namespace Ultramaverick.Quality.Persistence.Repositories;

public sealed class QualityUnitOfWork : IQualityUnitOfWork
{
    private readonly QualityDbContext _context;

    public QualityUnitOfWork(QualityDbContext context)
    {
        _context = context;

        QcReceipts = new QcReceiptRepository(context);
    }

    public IQcReceiptRepository QcReceipts { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}
