using Ultramaverick.Laboratory.Domain.Entities;
using Ultramaverick.Laboratory.Persistence;

namespace Ultramaverick.Laboratory.IntegrationTests;

internal static class TestData
{
    /// <summary>
    /// Creates a laboratory test request so exactly its Created event remains in the outbox.
    /// Returns the new request id.
    /// </summary>
    public static async Task<int> CreateLabTestRequestAsync(LaboratoryDbContext context)
    {
        var request = LabTestRequest.Create(
            warehouseReceiptId: null, batchNo: Code("LOT"), quantity: 5m, testType: "Micro",
            dateRequested: null, dateNeeded: null, requestedByUserId: 1);

        context.LabTestRequests.Add(request);
        await context.SaveChangesAsync();

        return request.Id;
    }

    public static async Task ClearOutboxAsync(LaboratoryDbContext context)
    {
        context.OutboxMessages.RemoveRange(context.OutboxMessages);
        await context.SaveChangesAsync();
    }

    public static string Code(string prefix) => $"{prefix}{Guid.NewGuid():N}"[..12];
}
