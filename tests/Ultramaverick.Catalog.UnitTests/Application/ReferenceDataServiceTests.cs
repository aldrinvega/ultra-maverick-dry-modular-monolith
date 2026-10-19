using Ultramaverick.Catalog.Application.Services;
using Ultramaverick.Catalog.UnitTests.Fakes;

namespace Ultramaverick.Catalog.UnitTests.Application;

public class ReferenceDataServiceTests
{
    private readonly FakeCatalogUnitOfWork _uow = new();
    private readonly ReferenceDataService _service;

    public ReferenceDataServiceTests()
        => _service = new ReferenceDataService(_uow, new FakeCurrentUser());

    [Fact]
    public async Task Item_category_create_update_and_deactivate_round_trip()
    {
        var created = await _service.CreateAsync("itemCategory", " Packaging ", null, default);
        Assert.True(created.Succeeded);

        var item = Assert.Single(_uow.ItemCategoriesRepo.All);
        Assert.Equal("Packaging", item.Name);

        var updated = await _service.UpdateAsync("itemCategory", item.Id, "Packing", null, default);
        Assert.True(updated.Succeeded);
        Assert.Equal("Packing", item.Name);

        var deactivated = await _service.SetActiveAsync("itemCategory", item.Id, false, default);
        Assert.True(deactivated.Succeeded);
        Assert.False(item.IsActive);
        Assert.Equal(3, _uow.SaveCount);
    }

    [Fact]
    public async Task A_blank_name_is_rejected_without_saving()
    {
        var result = await _service.CreateAsync("itemCategory", "   ", null, default);

        Assert.False(result.Succeeded);
        Assert.Empty(_uow.ItemCategoriesRepo.All);
        Assert.Equal(0, _uow.SaveCount);
    }
}
