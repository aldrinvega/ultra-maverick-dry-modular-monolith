using Ultramaverick.Catalog.Domain.Entities;
using Ultramaverick.Catalog.Persistence.Repositories;

namespace Ultramaverick.Catalog.IntegrationTests;

/// <summary>
/// Covers the generic lookup repository used by the reference tables that have no
/// dedicated repository.
/// </summary>
public class LookupRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public LookupRepositoryTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task ListAsync_filters_by_active_and_search_and_counts()
    {
        await using var context = _fixture.CreateContext();
        var repository = new LookupRepository<ItemCategory>(context);

        var prefix = $"Cat{Guid.NewGuid():N}"[..10];
        context.ItemCategories.Add(ItemCategory.Create($"{prefix}Alpha", null));
        context.ItemCategories.Add(ItemCategory.Create($"{prefix}Beta", null));

        var inactive = ItemCategory.Create($"{prefix}Gamma", null);
        inactive.SetActive(false, 1);
        context.ItemCategories.Add(inactive);

        await context.SaveChangesAsync();

        var active = await repository.ListAsync(prefix, true, 0, 10, CancellationToken.None);
        var total = await repository.CountAsync(prefix, null, CancellationToken.None);

        Assert.Equal(2, active.Count);
        Assert.All(active, category => Assert.True(category.IsActive));
        Assert.Equal(3, total);
    }

    [Fact]
    public async Task GetByNameAsync_returns_the_matching_row()
    {
        await using var context = _fixture.CreateContext();
        var repository = new LookupRepository<ItemCategory>(context);

        var name = $"Cat{Guid.NewGuid():N}"[..20];
        context.ItemCategories.Add(ItemCategory.Create(name, null));
        await context.SaveChangesAsync();

        var found = await repository.GetByNameAsync(name, CancellationToken.None);

        Assert.NotNull(found);
        Assert.Equal(name, found!.Name);
    }
}