using Ultramaverick.Catalog.Domain.Entities;
using Ultramaverick.Catalog.Persistence;

namespace Ultramaverick.Catalog.IntegrationTests;

internal static class TestData
{
    /// <summary>
    /// Creates the rows an item depends on, clears the outbox, then creates the item so
    /// exactly the item's event remains. Returns the new item id.
    /// </summary>
    public static async Task<int> CreateItemAsync(CatalogDbContext context)
    {
        var category = ItemCategory.Create($"Cat_{Guid.NewGuid():N}", null);
        context.ItemCategories.Add(category);

        var uom = Uom.Create(Code("U"), "Kilogram", null);
        context.Uoms.Add(uom);

        await context.SaveChangesAsync();
        await ClearOutboxAsync(context);

        var item = Item.Create(
            Code("I"), "Test Item", category.Id, uom.Id, 10m, false, null);

        context.Items.Add(item);
        await context.SaveChangesAsync();

        return item.Id;
    }

    public static async Task ClearOutboxAsync(CatalogDbContext context)
    {
        context.OutboxMessages.RemoveRange(context.OutboxMessages);
        await context.SaveChangesAsync();
    }

    private static string Code(string prefix) => $"{prefix}{Guid.NewGuid():N}"[..12];
}