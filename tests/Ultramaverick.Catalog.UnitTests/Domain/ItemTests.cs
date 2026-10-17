using Ultramaverick.Catalog.Domain.Entities;
using Ultramaverick.Catalog.Domain.Events;

namespace Ultramaverick.Catalog.UnitTests.Domain;

public class ItemTests
{
    [Fact]
    public void Create_trims_text_sets_active_and_raises_created()
    {
        var item = Item.Create("  RAW-1  ", "  Raw Material  ", 1, 2, 5m, true, 7);

        Assert.Equal("RAW-1", item.ItemCode);
        Assert.Equal("Raw Material", item.ItemDescription);
        Assert.True(item.IsActive);
        Assert.Equal(7, item.CreatedByUserId);

        var raised = Assert.IsType<ItemChanged>(item.DomainEvents.First());
        Assert.Equal(ItemChangeTypes.Created, raised.ChangeType);
    }

    [Theory]
    [InlineData("", "Raw Material")]
    [InlineData("   ", "Raw Material")]
    [InlineData("RAW-1", "")]
    [InlineData("RAW-1", "   ")]
    public void Create_rejects_blank_text(string code, string description)
        => Assert.Throws<ArgumentException>(() => Item.Create(code, description, 1, 2, null, false, null));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_non_positive_category(int categoryId)
        => Assert.Throws<ArgumentOutOfRangeException>(
            () => Item.Create("RAW-1", "Raw Material", categoryId, 2, null, false, null));

    [Fact]
    public void SetActive_is_idempotent_and_raises_once()
    {
        var item = Item.Create("RAW-1", "Raw Material", 1, 2, null, false, null);
        item.ClearDomainEvents();

        item.SetActive(false, 9);
        item.SetActive(false, 9);

        Assert.False(item.IsActive);
        Assert.Single(item.DomainEvents);
        Assert.Equal(ItemChangeTypes.Deactivated, ((ItemChanged)item.DomainEvents.First()).ChangeType);
    }

    [Fact]
    public void Update_raises_updated()
    {
        var item = Item.Create("RAW-1", "Raw Material", 1, 2, null, false, null);
        item.ClearDomainEvents();

        item.Update("  Renamed  ", 3, 4, 12m, true, 9);

        Assert.Equal("Renamed", item.ItemDescription);
        Assert.Equal(3, item.ItemCategoryId);
        Assert.Equal(12m, item.BufferLevel);
        Assert.Equal(ItemChangeTypes.Updated, ((ItemChanged)item.DomainEvents.First()).ChangeType);
    }
}