using Ultramaverick.Reporting.Application.Abstractions;
using Ultramaverick.Reporting.Application.EventHandlers;
using Ultramaverick.Reporting.Domain.Entities;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Reporting.UnitTests.Application;

internal sealed class FakeProcessedEventStore : IProcessedEventStore
{
    private readonly HashSet<(Guid, string)> _processed = new();

    public Task<bool> HasProcessedAsync(Guid eventId, string projection, CancellationToken ct)
        => Task.FromResult(_processed.Contains((eventId, projection)));

    public Task MarkProcessedAsync(Guid eventId, string projection, CancellationToken ct)
    {
        _processed.Add((eventId, projection));
        return Task.CompletedTask;
    }
}

internal sealed class FakeInventoryPositionStore : IInventoryPositionStore
{
    private readonly List<InventoryPosition> _rows = new();

    public Task<InventoryPosition?> FindAsync(int warehouseId, int itemId, string lotNo, CancellationToken ct)
        => Task.FromResult(_rows.FirstOrDefault(
            x => x.WarehouseId == warehouseId && x.ItemId == itemId && x.LotNo == lotNo));

    public Task AddAsync(InventoryPosition position, CancellationToken ct)
    {
        _rows.Add(position);
        return Task.CompletedTask;
    }
}

internal sealed class FakeOrderFulfillmentStore : IOrderFulfillmentStore
{
    private readonly List<OrderFulfillment> _rows = new();

    public Task<OrderFulfillment?> FindAsync(string orderNumber, CancellationToken ct)
        => Task.FromResult(_rows.FirstOrDefault(x => x.OrderNumber == orderNumber));

    public Task AddAsync(OrderFulfillment fulfillment, CancellationToken ct)
    {
        _rows.Add(fulfillment);
        return Task.CompletedTask;
    }
}

internal sealed class FakeQcStatusStore : IQcStatusStore
{
    private readonly List<QcStatus> _rows = new();

    public Task<QcStatus?> FindAsync(int qcReceiptId, CancellationToken ct)
        => Task.FromResult(_rows.FirstOrDefault(x => x.QcReceiptId == qcReceiptId));

    public Task AddAsync(QcStatus status, CancellationToken ct)
    {
        _rows.Add(status);
        return Task.CompletedTask;
    }
}

internal sealed class FakeReportingUnitOfWork : IReportingUnitOfWork
{
    public IInventoryPositionStore InventoryPositions { get; } = new FakeInventoryPositionStore();
    public IOrderFulfillmentStore OrderFulfillment { get; } = new FakeOrderFulfillmentStore();
    public IQcStatusStore QcStatus { get; } = new FakeQcStatusStore();

    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        SaveCount++;
        return Task.FromResult(1);
    }
}

public class ProjectionHandlerTests
{
    private static readonly DateTime OccurredAt = new(2026, 10, 18, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task A_stock_received_event_is_applied_once_per_event_id()
    {
        var uow = new FakeReportingUnitOfWork();
        var processed = new FakeProcessedEventStore();
        var handler = new StockReceivedHandler(uow, processed);
        var eventId = Guid.NewGuid();

        var notification = new StockReceivedNotification(1, 10, 20, "LOT-1", 5m, OccurredAt, eventId);

        await handler.Handle(notification, CancellationToken.None);
        await handler.Handle(notification, CancellationToken.None);

        Assert.Equal(1, uow.SaveCount);

        var position = await uow.InventoryPositions.FindAsync(1, 10, "LOT-1", CancellationToken.None);
        Assert.NotNull(position);
        Assert.Equal(5m, position.Quantity);
    }

    [Fact]
    public async Task Two_distinct_receipts_for_the_same_lot_accumulate()
    {
        var uow = new FakeReportingUnitOfWork();
        var processed = new FakeProcessedEventStore();
        var handler = new StockReceivedHandler(uow, processed);

        await handler.Handle(
            new StockReceivedNotification(1, 10, 20, "LOT-1", 5m, OccurredAt, Guid.NewGuid()),
            CancellationToken.None);

        await handler.Handle(
            new StockReceivedNotification(1, 10, 20, "LOT-1", 3m, OccurredAt, Guid.NewGuid()),
            CancellationToken.None);

        var position = await uow.InventoryPositions.FindAsync(1, 10, "LOT-1", CancellationToken.None);
        Assert.NotNull(position);
        Assert.Equal(8m, position.Quantity);
    }

    [Fact]
    public async Task A_confirmed_order_updates_the_existing_fulfillment_row()
    {
        var uow = new FakeReportingUnitOfWork();
        var processed = new FakeProcessedEventStore();

        await new OrderCreatedHandler(uow, processed).Handle(
            new OrderCreatedNotification("SO-1", 7, OccurredAt, Guid.NewGuid()), CancellationToken.None);

        await new OrderConfirmedHandler(uow, processed).Handle(
            new OrderConfirmedNotification("SO-1", OccurredAt.AddMinutes(1), Guid.NewGuid()),
            CancellationToken.None);

        var fulfillment = await uow.OrderFulfillment.FindAsync("SO-1", CancellationToken.None);
        Assert.NotNull(fulfillment);
        Assert.Equal("Confirmed", fulfillment.Status);
        Assert.Equal(7, fulfillment.CustomerId);
    }

    [Fact]
    public async Task A_qc_receipt_is_projected_once()
    {
        var uow = new FakeReportingUnitOfWork();
        var processed = new FakeProcessedEventStore();
        var handler = new QcReceiptCompletedHandler(uow, processed);
        var eventId = Guid.NewGuid();

        var notification = new QcReceiptCompletedNotification(5, 10, 20, "LOT-1", 100m, OccurredAt, eventId);

        await handler.Handle(notification, CancellationToken.None);
        await handler.Handle(notification, CancellationToken.None);

        Assert.Equal(1, uow.SaveCount);

        var status = await uow.QcStatus.FindAsync(5, CancellationToken.None);
        Assert.NotNull(status);
        Assert.Equal(100m, status.NetQuantity);
    }
}
