using Ultramaverick.Procurement.Domain.Entities;
using Ultramaverick.Procurement.Persistence;

namespace Ultramaverick.Procurement.IntegrationTests;

internal static class TestData
{
    /// <summary>
    /// Creates a purchase order so exactly its Created event remains in the outbox.
    /// Returns the new purchase order id.
    /// </summary>
    public static async Task<int> CreatePurchaseOrderAsync(ProcurementDbContext context)
    {
        var po = PurchaseOrder.Create(
            Code("PO"), null, null, null, supplierId: 1, importBatchId: null, createdByUserId: null);

        context.PurchaseOrders.Add(po);
        await context.SaveChangesAsync();

        return po.Id;
    }

    public static async Task ClearOutboxAsync(ProcurementDbContext context)
    {
        context.OutboxMessages.RemoveRange(context.OutboxMessages);
        await context.SaveChangesAsync();
    }

    private static string Code(string prefix) => $"{prefix}{Guid.NewGuid():N}"[..12];
}
