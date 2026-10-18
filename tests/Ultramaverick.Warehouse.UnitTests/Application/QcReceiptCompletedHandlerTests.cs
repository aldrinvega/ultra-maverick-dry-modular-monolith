using Ultramaverick.Warehouse.Application.EventHandlers;
using Ultramaverick.Warehouse.Application.Services;
using Ultramaverick.Warehouse.Domain;
using Ultramaverick.Warehouse.Domain.Entities;
using Ultramaverick.Warehouse.UnitTests.Fakes;

namespace Ultramaverick.Warehouse.UnitTests.Application;

public class QcReceiptCompletedHandlerTests
{
    private static (QcReceiptCompletedHandler Handler, FakeWarehouseUnitOfWork Uow,
        FakeProcessedEventStore Processed) Build()
    {
        var uow = new FakeWarehouseUnitOfWork();
        var processed = new FakeProcessedEventStore();
        var handler = new QcReceiptCompletedHandler(
            uow, new InventoryLedger(uow), new FakeWarehouseSelector { WarehouseId = 3 }, processed);
        return (handler, uow, processed);
    }

    private static QcReceiptCompletedNotification Notification(int qcReceiptId, Guid eventId)
        => new(qcReceiptId, ItemId: 10, UomId: 20, LotNo: "LOT-1", NetQuantity: 5m,
            ManufacturingDate: null, ExpiryDate: null, CompletedBy: 7, eventId);

    [Fact]
    public async Task Handle_receipts_stock_into_the_default_warehouse_and_saves_once()
    {
        var (handler, uow, processed) = Build();

        await handler.Handle(Notification(55, Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(1, uow.SaveCount);
        Assert.Equal(1, processed.MarkCount);

        var receipt = Assert.Single(uow.ReceiptsRepo.All);
        Assert.Equal(55, receipt.QcReceiptId);
        Assert.Equal(3, receipt.WarehouseId);
        Assert.Equal(5m, receipt.Quantity);

        var movement = Assert.Single(uow.MovementsRepo.All);
        Assert.Equal(MovementSource.QcReceipt, movement.Source);
        Assert.Equal(55, movement.SourceDocumentId);
        Assert.Equal(5m, movement.Delta);

        Assert.Equal(5m, Assert.Single(uow.BalancesRepo.All).OnHand);
    }

    [Fact]
    public async Task Handle_is_a_no_op_when_a_receipt_exists_for_the_qc_receipt()
    {
        var (handler, uow, processed) = Build();
        uow.ReceiptsRepo.Seed(WarehouseReceipt.Create(
            qcReceiptId: 55, warehouseId: 3, itemId: 10, uomId: 20, lotNo: null, quantity: 1m,
            manufacturingDate: null, expiryDate: null, receivedBy: null));

        await handler.Handle(Notification(55, Guid.NewGuid()), CancellationToken.None);

        Assert.Single(uow.ReceiptsRepo.All);
        Assert.Equal(0, uow.SaveCount);
        Assert.Equal(0, processed.MarkCount);
    }

    [Fact]
    public async Task Handle_is_a_no_op_when_the_event_was_already_processed()
    {
        var (handler, uow, processed) = Build();
        var eventId = Guid.NewGuid();
        await processed.MarkProcessedAsync(
            eventId, nameof(QcReceiptCompletedHandler), CancellationToken.None);

        await handler.Handle(Notification(55, eventId), CancellationToken.None);

        Assert.Empty(uow.ReceiptsRepo.All);
        Assert.Equal(0, uow.SaveCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Handle_records_a_non_positive_quantity_event_without_writing_stock(decimal netQuantity)
    {
        var (handler, uow, processed) = Build();
        var eventId = Guid.NewGuid();
        var notification = new QcReceiptCompletedNotification(
            56, ItemId: 10, UomId: 20, LotNo: "LOT-Z", NetQuantity: netQuantity,
            ManufacturingDate: null, ExpiryDate: null, CompletedBy: 7, eventId);

        await handler.Handle(notification, CancellationToken.None);

        Assert.Equal(1, processed.MarkCount);
        Assert.Equal(1, uow.SaveCount);
        Assert.Empty(uow.ReceiptsRepo.All);
        Assert.Empty(uow.BalancesRepo.All);
        Assert.Empty(uow.MovementsRepo.All);
    }
}
