using Ultramaverick.Quality.Application.Abstractions;
using Ultramaverick.Quality.Application.EventHandlers;
using Ultramaverick.Quality.UnitTests.Fakes;

namespace Ultramaverick.Quality.UnitTests.Application;

public class PurchaseOrderReceivedHandlerTests
{
    private static (PurchaseOrderReceivedHandler Handler, FakeQualityUnitOfWork Uow,
        FakePurchaseOrderLineLookup Lines, FakeProcessedEventStore Processed) Build()
    {
        var uow = new FakeQualityUnitOfWork();
        var lines = new FakePurchaseOrderLineLookup();
        var processed = new FakeProcessedEventStore();
        var handler = new PurchaseOrderReceivedHandler(uow, lines, processed);
        return (handler, uow, lines, processed);
    }

    private static PurchaseOrderReceivedNotification Notification(int purchaseOrderId, params int[] lineIds)
        => new(purchaseOrderId, SupplierId: 5, lineIds, Guid.NewGuid());

    [Fact]
    public async Task Handle_creates_one_receipt_per_line_and_saves_once()
    {
        var (handler, uow, lines, processed) = Build();
        lines.Lines[1] = new PurchaseOrderLineInfo(1, 10, 20, 5m, null, null);
        lines.Lines[2] = new PurchaseOrderLineInfo(2, 11, 21, 3m, null, null);

        await handler.Handle(Notification(100, 1, 2), CancellationToken.None);

        Assert.Equal(2, uow.QcReceiptsRepo.All.Count);
        Assert.Equal(1, uow.SaveCount);
        Assert.Equal(1, processed.MarkCount);
    }

    [Fact]
    public async Task Handle_is_a_no_op_when_the_purchase_order_already_has_receipts()
    {
        var (handler, uow, lines, processed) = Build();
        lines.Lines[1] = new PurchaseOrderLineInfo(1, 10, 20, 5m, null, null);
        uow.QcReceiptsRepo.Seed(Ultramaverick.Quality.Domain.Entities.QcReceipt.Create(100, 1, null, null, 1m, 1));

        await handler.Handle(Notification(100, 1), CancellationToken.None);

        Assert.Single(uow.QcReceiptsRepo.All);
        Assert.Equal(0, uow.SaveCount);
        Assert.Equal(0, processed.MarkCount);
    }

    [Fact]
    public async Task Handle_is_a_no_op_when_the_event_was_already_processed()
    {
        var (handler, uow, lines, processed) = Build();
        lines.Lines[1] = new PurchaseOrderLineInfo(1, 10, 20, 5m, null, null);

        var eventId = Guid.NewGuid();
        await processed.MarkProcessedAsync(eventId, nameof(PurchaseOrderReceivedHandler), CancellationToken.None);

        await handler.Handle(new PurchaseOrderReceivedNotification(100, 5, new[] { 1 }, eventId), CancellationToken.None);

        Assert.Empty(uow.QcReceiptsRepo.All);
        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task Handle_throws_when_a_line_cannot_be_found()
    {
        var (handler, _, _, _) = Build();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(Notification(100, 1), CancellationToken.None));
    }
}
