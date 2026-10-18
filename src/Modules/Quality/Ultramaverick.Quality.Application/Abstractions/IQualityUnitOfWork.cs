namespace Ultramaverick.Quality.Application.Abstractions;

/// <summary>Aggregates every Quality repository behind one transaction boundary.</summary>
public interface IQualityUnitOfWork
{
    IQcReceiptRepository QcReceipts { get; }

    Task<int> SaveChangesAsync(CancellationToken ct);
}
