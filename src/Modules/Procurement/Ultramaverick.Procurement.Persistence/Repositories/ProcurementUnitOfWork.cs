using Ultramaverick.Procurement.Application.Abstractions;

namespace Ultramaverick.Procurement.Persistence.Repositories;

public sealed class ProcurementUnitOfWork : IProcurementUnitOfWork
{
    private readonly ProcurementDbContext _context;

    public ProcurementUnitOfWork(ProcurementDbContext context)
    {
        _context = context;

        PurchaseOrders = new PurchaseOrderRepository(context);
    }

    public IPurchaseOrderRepository PurchaseOrders { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}
