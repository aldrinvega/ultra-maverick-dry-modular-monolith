using Ultramaverick.Warehouse.Application.Commands.Inventory;
using Ultramaverick.Warehouse.Application.Services;
using Ultramaverick.Warehouse.Domain;
using Ultramaverick.Warehouse.Domain.Entities;
using Ultramaverick.Warehouse.UnitTests.Fakes;

namespace Ultramaverick.Warehouse.UnitTests.Application;

public class AdjustStockCommandHandlerTests
{
    [Fact]
    public async Task Handle_posts_an_adjustment_movement_and_saves_once()
    {
        var handler = Build(withBalance: 5m, out var uow);

        var result = await handler.Handle(
            new AdjustStockCommand(1, 10, 20, null, -2m), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, uow.SaveCount);

        var movement = Assert.Single(uow.MovementsRepo.All);
        Assert.Equal(MovementSource.Adjustment, movement.Source);
        Assert.Null(movement.SourceDocumentId);
        Assert.Equal(-2m, movement.Delta);
        Assert.Equal(3m, Assert.Single(uow.BalancesRepo.All).OnHand);
    }

    [Fact]
    public async Task Handle_fails_and_does_not_save_when_stock_would_go_negative()
    {
        var handler = Build(withBalance: 1m, out var uow);

        var result = await handler.Handle(
            new AdjustStockCommand(1, 10, 20, null, -2m), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal(0, uow.SaveCount);
        Assert.Empty(uow.MovementsRepo.All);
    }

    [Fact]
    public async Task Handle_fails_when_no_balance_exists_for_an_issue()
    {
        var uow = new FakeWarehouseUnitOfWork();
        var handler = new AdjustStockCommandHandler(uow, new InventoryLedger(uow));

        var result = await handler.Handle(
            new AdjustStockCommand(1, 10, 20, null, -1m), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, uow.SaveCount);
    }

    private static AdjustStockCommandHandler Build(decimal withBalance, out FakeWarehouseUnitOfWork uow)
    {
        uow = new FakeWarehouseUnitOfWork();
        var balance = InventoryBalance.Create(1, 10, 20, null);
        balance.Apply(withBalance);
        uow.BalancesRepo.Seed(balance);

        return new AdjustStockCommandHandler(uow, new InventoryLedger(uow));
    }
}
