using Ultramaverick.Reporting.Application.Abstractions;

namespace Ultramaverick.Reporting.Persistence.Repositories;

public sealed class ReportingUnitOfWork : IReportingUnitOfWork
{
    private readonly ReportingDbContext _context;

    public ReportingUnitOfWork(ReportingDbContext context)
    {
        _context = context;

        InventoryPositions = new InventoryPositionStore(context);
        OrderFulfillment = new OrderFulfillmentStore(context);
        QcStatus = new QcStatusStore(context);
    }

    public IInventoryPositionStore InventoryPositions { get; }

    public IOrderFulfillmentStore OrderFulfillment { get; }

    public IQcStatusStore QcStatus { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}
