using Ultramaverick.Warehouse.Application.Commands.WarehouseReceipts;
using Ultramaverick.Warehouse.Application.Services;
using Ultramaverick.Warehouse.Domain;
using Ultramaverick.Warehouse.Domain.Entities;
using Ultramaverick.Warehouse.UnitTests.Fakes;

namespace Ultramaverick.Warehouse.UnitTests.Application;

public class CreateWarehouseReceiptCommandHandlerTests
{
    [Fact]
    public async Task Handle_creates_a_receipt_posts_a_misc_receipt_movement_and_saves_once()
    {
        var uow = new FakeWarehouseUnitOfWork();
        var handler = new CreateWarehouseReceiptCommandHandler(
            uow, new InventoryLedger(uow), new FakeCurrentUser { UserId = 7 });

        var result = await handler.Handle(
            new CreateWarehouseReceiptCommand(1, 10, 20, "LOT-1", 5m, null, null),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, uow.SaveCount);

        var receipt = Assert.Single(uow.ReceiptsRepo.All);
        Assert.Null(receipt.QcReceiptId);
        Assert.Equal(5m, receipt.Quantity);
        Assert.Equal(7, receipt.ReceivedByUserId);

        var movement = Assert.Single(uow.MovementsRepo.All);
        Assert.Equal(MovementSource.MiscReceipt, movement.Source);
        Assert.Null(movement.SourceDocumentId);
        Assert.Equal(5m, movement.Delta);

        Assert.Equal(5m, Assert.Single(uow.BalancesRepo.All).OnHand);
    }

    [Fact]
    public async Task Handle_folds_into_an_existing_balance()
    {
        var uow = new FakeWarehouseUnitOfWork();
        var existing = InventoryBalance.Create(1, 10, 20, "LOT-1");
        existing.Apply(2m);
        uow.BalancesRepo.Seed(existing);
        var handler = new CreateWarehouseReceiptCommandHandler(
            uow, new InventoryLedger(uow), new FakeCurrentUser());

        await handler.Handle(
            new CreateWarehouseReceiptCommand(1, 10, 20, "LOT-1", 3m, null, null),
            CancellationToken.None);

        Assert.Equal(5m, existing.OnHand);
        Assert.Single(uow.BalancesRepo.All);
    }
}
