using Ultramaverick.Quality.Application.Abstractions;
using Ultramaverick.Quality.Domain.Entities;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Quality.UnitTests.Fakes;

internal sealed class FakeQcReceiptRepository : IQcReceiptRepository
{
    private readonly List<QcReceipt> _receipts = new();
    private readonly Dictionary<int, QcReceipt> _byId = new();

    public IReadOnlyList<QcReceipt> All => _receipts;

    public void Seed(QcReceipt receipt) => _receipts.Add(receipt);

    public void SeedWithId(int id, QcReceipt receipt)
    {
        _byId[id] = receipt;
        _receipts.Add(receipt);
    }

    public Task<bool> ExistsForPurchaseOrderAsync(int purchaseOrderId, CancellationToken ct)
        => Task.FromResult(_receipts.Any(r => r.PurchaseOrderId == purchaseOrderId));

    public Task<QcReceipt?> GetByIdAsync(int qcReceiptId, CancellationToken ct)
        => Task.FromResult(_byId.TryGetValue(qcReceiptId, out var receipt) ? receipt : null);

    public Task<IReadOnlyList<QcReceipt>> ListAsync(string? search, int skip, int take, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<QcReceipt>>(_receipts.Skip(skip).Take(take).ToList());

    public Task<int> CountAsync(string? search, CancellationToken ct)
        => Task.FromResult(_receipts.Count);

    public Task AddAsync(QcReceipt entity, CancellationToken ct)
    {
        _receipts.Add(entity);
        return Task.CompletedTask;
    }
}

internal sealed class FakeQualityUnitOfWork : IQualityUnitOfWork
{
    public FakeQcReceiptRepository QcReceiptsRepo { get; } = new();

    public int SaveCount { get; private set; }

    public IQcReceiptRepository QcReceipts => QcReceiptsRepo;

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        SaveCount++;
        return Task.FromResult(SaveCount);
    }
}

internal sealed class FakePurchaseOrderLineLookup : IPurchaseOrderLineLookup
{
    public Dictionary<int, PurchaseOrderLineInfo> Lines { get; } = new();

    public Task<PurchaseOrderLineInfo?> GetAsync(int purchaseOrderLineId, CancellationToken ct)
        => Task.FromResult(Lines.TryGetValue(purchaseOrderLineId, out var line) ? line : null);
}

internal sealed class FakeProcessedEventStore : IProcessedEventStore
{
    private readonly HashSet<(Guid EventId, string Projection)> _processed = new();

    public int MarkCount { get; private set; }

    public Task<bool> HasProcessedAsync(Guid eventId, string projection, CancellationToken ct)
        => Task.FromResult(_processed.Contains((eventId, projection)));

    public Task MarkProcessedAsync(Guid eventId, string projection, CancellationToken ct)
    {
        _processed.Add((eventId, projection));
        MarkCount++;
        return Task.CompletedTask;
    }
}

internal sealed class FakeCurrentUser : ICurrentUser
{
    public int? UserId { get; set; } = 1;
    public string? UserName { get; set; } = "tester";
    public bool IsAuthenticated { get; set; } = true;
}
