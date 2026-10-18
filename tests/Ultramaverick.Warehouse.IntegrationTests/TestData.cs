using System.Text.Json;
using Ultramaverick.Quality.Contracts;
using Ultramaverick.SharedKernel.Outbox;
using Ultramaverick.Warehouse.Persistence;
using WarehouseEntity = Ultramaverick.Warehouse.Domain.Entities.Warehouse;

namespace Ultramaverick.Warehouse.IntegrationTests;

internal static class TestData
{
    /// <summary>Creates an active warehouse and returns its id, so the selector has a target.</summary>
    public static async Task<int> CreateWarehouseAsync(WarehouseDbContext context)
    {
        var warehouse = WarehouseEntity.Create(Code("WH"), "Main Warehouse", null, null, null);

        context.Warehouses.Add(warehouse);
        await context.SaveChangesAsync();

        return warehouse.Id;
    }

    public static async Task ClearOutboxAsync(WarehouseDbContext context)
    {
        context.OutboxMessages.RemoveRange(context.OutboxMessages);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Builds the outbox record Quality publishes when a QC receipt completes, serialised with
    /// the same default options the Warehouse bridge deserialises with.
    /// </summary>
    public static OutboxRecord QcReceiptCompletedRecord(
        Guid eventId, int qcReceiptId, int itemId, int uomId, string? lotNo, decimal netQuantity)
    {
        var payload = JsonSerializer.Serialize(new QcReceiptCompleted(
            qcReceiptId, itemId, uomId, lotNo, netQuantity, null, null, 7, DateTime.UtcNow));

        return new OutboxRecord(
            eventId, "QcReceipt", qcReceiptId.ToString(), "QcReceiptCompleted", payload, Attempts: 0);
    }

    public static string Code(string prefix) => $"{prefix}{Guid.NewGuid():N}"[..12];
}
