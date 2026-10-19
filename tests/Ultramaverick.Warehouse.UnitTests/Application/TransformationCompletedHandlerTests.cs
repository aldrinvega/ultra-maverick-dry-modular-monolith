using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Application.EventHandlers;
using Ultramaverick.Warehouse.UnitTests.Fakes;

namespace Ultramaverick.Warehouse.UnitTests.Application;

internal sealed class FakeItemUomLookup : IItemUomLookup
{
    private readonly int? _uomId;

    public FakeItemUomLookup(int? uomId) => _uomId = uomId;

    public Task<int?> GetUomIdAsync(int itemId, CancellationToken ct) => Task.FromResult(_uomId);
}

public class TransformationCompletedHandlerTests
{
    private static TransformationCompletedNotification Notification(Guid eventId) => new(
        WarehouseId: 1,
        Lines: new[] { (ItemId: 10, Quantity: 5m), (ItemId: 11, Quantity: 2m) },
        EventId: eventId);

    [Fact]
    public async Task One_receipt_per_line_and_a_redelivery_adds_none()
    {
        var uow = new FakeWarehouseUnitOfWork();
        var handler = new TransformationCompletedHandler(
            uow, new FakeItemUomLookup(20), new FakeProcessedEventStore());

        var eventId = Guid.NewGuid();
        await handler.Handle(Notification(eventId), CancellationToken.None);
        await handler.Handle(Notification(eventId), CancellationToken.None);

        Assert.Equal(2, uow.ReceiptsRepo.All.Count);
        Assert.All(uow.ReceiptsRepo.All, receipt => Assert.Equal(20, receipt.UomId));
        Assert.All(uow.ReceiptsRepo.All, receipt => Assert.Equal(1, receipt.WarehouseId));
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task An_item_without_a_unit_of_measure_fails_loudly()
    {
        var uow = new FakeWarehouseUnitOfWork();
        var handler = new TransformationCompletedHandler(
            uow, new FakeItemUomLookup(null), new FakeProcessedEventStore());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(Notification(Guid.NewGuid()), CancellationToken.None));

        Assert.Empty(uow.ReceiptsRepo.All);
    }
}
