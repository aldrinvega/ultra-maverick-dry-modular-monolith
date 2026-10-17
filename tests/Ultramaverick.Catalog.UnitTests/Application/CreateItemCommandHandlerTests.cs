using Ultramaverick.Catalog.Application.Commands.Items;
using Ultramaverick.Catalog.Domain.Entities;
using Ultramaverick.Catalog.UnitTests.Fakes;

namespace Ultramaverick.Catalog.UnitTests.Application;

public class CreateItemCommandHandlerTests
{
    private static (CreateItemCommandHandler Handler, FakeCatalogUnitOfWork Uow) Build()
    {
        var uow = new FakeCatalogUnitOfWork();
        uow.ItemCategoriesRepo.Seed(ItemCategory.Create("Raw", null));
        var handler = new CreateItemCommandHandler(uow, new FakeCurrentUser { UserId = 7 });
        return (handler, uow);
    }

    [Fact]
    public async Task Handle_creates_the_item_and_saves_once()
    {
        var (handler, uow) = Build();
        uow.UomsRepo.Seed(Uom.Create("KG", "Kilogram", null));

        var result = await handler.Handle(
            new CreateItemCommand("RAW-1", "Raw Material", 1, 1, 5m, false), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, uow.SaveCount);
        Assert.Single(uow.ItemsRepo.All);
        Assert.Equal("RAW-1", uow.ItemsRepo.All[0].ItemCode);
    }

    [Fact]
    public async Task Handle_rejects_a_duplicate_item_code()
    {
        var (handler, uow) = Build();
        uow.UomsRepo.Seed(Uom.Create("KG", "Kilogram", null));
        await uow.ItemsRepo.AddAsync(Item.Create("RAW-1", "Existing", 1, 1, null, false, null), CancellationToken.None);

        var result = await handler.Handle(
            new CreateItemCommand("RAW-1", "Raw Material", 1, 1, null, false), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task Handle_rejects_a_missing_uom()
    {
        var (handler, uow) = Build();

        var result = await handler.Handle(
            new CreateItemCommand("RAW-1", "Raw Material", 1, 99, null, false), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, uow.SaveCount);
    }
}