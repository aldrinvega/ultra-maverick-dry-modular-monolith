using Microsoft.EntityFrameworkCore;
using Ultramaverick.Manufacturing.Domain.Entities;
using Ultramaverick.Manufacturing.Persistence;

namespace Ultramaverick.Manufacturing.IntegrationTests;

internal static class TestData
{
    /// <summary>
    /// Seeds a production plan row. ProductionPlan is map-only this wave (no factory), so the
    /// seed goes through raw SQL rather than the domain surface.
    /// </summary>
    public static async Task<int> CreateProductionPlanAsync(ManufacturingDbContext context)
    {
        await context.Database.ExecuteSqlAsync($"""
            INSERT INTO [Manufacturing].[ProductionPlans]
                ([ItemId], [UomId], [Quantity], [Status], [IsApproved], [IsPrepared], [IsMixed], [IsActive], [CreatedAtUtc])
            VALUES (10, 20, 100, 'Pending', 0, 0, 0, 1, SYSUTCDATETIME());
            """);

        return await context.ProductionPlans
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Select(x => x.Id)
            .FirstAsync();
    }

    /// <summary>Creates a transformation request and returns its id.</summary>
    public static async Task<int> CreateRequestAsync(ManufacturingDbContext context, int productionPlanId)
    {
        var request = TransformationRequest.Create(
            productionPlanId,
            warehouseId: 1,
            createdByUserId: 7,
            lines: new[] { new TransformationLineInput(ItemId: 10, RequiredQuantity: 5m) });

        context.TransformationRequests.Add(request);
        await context.SaveChangesAsync();

        return request.Id;
    }

    public static async Task ClearOutboxAsync(ManufacturingDbContext context)
    {
        context.OutboxMessages.RemoveRange(context.OutboxMessages);
        await context.SaveChangesAsync();
    }
}
