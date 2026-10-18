using Ultramaverick.Warehouse.Application.Commands.Warehouses;
using Ultramaverick.Warehouse.UnitTests.Fakes;
using WarehouseEntity = Ultramaverick.Warehouse.Domain.Entities.Warehouse;

namespace Ultramaverick.Warehouse.UnitTests.Application;

public class CreateWarehouseCommandHandlerTests
{
    [Fact]
    public async Task Handle_creates_the_warehouse_and_saves_once()
    {
        var uow = new FakeWarehouseUnitOfWork();
        var handler = new CreateWarehouseCommandHandler(uow, new FakeCurrentUser { UserId = 7 });

        var result = await handler.Handle(
            new CreateWarehouseCommand("WH-1", "Main", 3, "123 Depot Rd"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, uow.SaveCount);

        var entity = Assert.Single(uow.WarehousesRepo.All);
        Assert.Equal("WH-1", entity.Code);
        Assert.Equal("Main", entity.Name);
        Assert.Equal(3, entity.WarehouseTypeId);
        Assert.Equal(7, entity.CreatedByUserId);
        Assert.Single(entity.DomainEvents);
    }

    [Fact]
    public async Task Handle_fails_when_the_code_already_exists()
    {
        var uow = new FakeWarehouseUnitOfWork();
        uow.WarehousesRepo.Seed(WarehouseEntity.Create("WH-1", "Existing", null, null, null));
        var handler = new CreateWarehouseCommandHandler(uow, new FakeCurrentUser());

        var result = await handler.Handle(
            new CreateWarehouseCommand("WH-1", "Main", null, null), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal(0, uow.SaveCount);
        Assert.Single(uow.WarehousesRepo.All);
    }
}
