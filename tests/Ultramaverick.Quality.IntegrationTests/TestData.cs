using Ultramaverick.Procurement.Domain.Entities;
using Ultramaverick.Procurement.Persistence;
using Ultramaverick.Quality.Domain.Entities;
using Ultramaverick.Quality.Persistence;

namespace Ultramaverick.Quality.IntegrationTests;

internal static class TestData
{
    /// <summary>
    /// Creates a QC receipt so exactly its Created event remains in the outbox.
    /// Returns the new receipt id.
    /// </summary>
    public static async Task<int> CreateQcReceiptAsync(QualityDbContext context)
    {
        var receipt = QcReceipt.Create(
            purchaseOrderId: 1, itemId: 10, productTypeId: null, batchNo: Code("LOT"),
            actualDelivered: 5m, uomId: 20);

        context.QcReceipts.Add(receipt);
        await context.SaveChangesAsync();

        return receipt.Id;
    }

    public static async Task ClearOutboxAsync(QualityDbContext context)
    {
        context.OutboxMessages.RemoveRange(context.OutboxMessages);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a purchase order with one line and returns (purchaseOrderId, lineId).
    /// </summary>
    public static async Task<(int PurchaseOrderId, int LineId)> CreatePurchaseOrderWithLineAsync(
        ProcurementDbContext context)
    {
        var po = PurchaseOrder.Create(Code("PO"), null, null, null, supplierId: 5, null, null);
        var line = po.AddLine(itemId: 10, uomId: 20, orderedQuantity: 5m, unitPrice: 1m);

        context.PurchaseOrders.Add(po);
        await context.SaveChangesAsync();

        return (po.Id, line.Id);
    }

    public static string Code(string prefix) => $"{prefix}{Guid.NewGuid():N}"[..12];
}
