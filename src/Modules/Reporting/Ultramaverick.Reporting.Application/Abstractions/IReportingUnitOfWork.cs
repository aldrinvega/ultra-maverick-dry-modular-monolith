namespace Ultramaverick.Reporting.Application.Abstractions;

public interface IReportingUnitOfWork
{
    IInventoryPositionStore InventoryPositions { get; }
    IOrderFulfillmentStore OrderFulfillment { get; }
    IQcStatusStore QcStatus { get; }
    Task<int> SaveChangesAsync(CancellationToken ct);
}
