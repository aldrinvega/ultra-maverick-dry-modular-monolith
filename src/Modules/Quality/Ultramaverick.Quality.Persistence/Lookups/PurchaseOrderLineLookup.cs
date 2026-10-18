using Microsoft.EntityFrameworkCore;
using Ultramaverick.Quality.Application.Abstractions;

namespace Ultramaverick.Quality.Persistence.Lookups;

/// <summary>
/// Reads Procurement's [PurchaseOrderLines] cross-schema so Quality can build a QC receipt from
/// a purchase-order line without a project reference to Procurement. Read-only query.
///
/// Deviation: the guide's port returns ProductTypeId and BatchNo, but
/// [Procurement].[PurchaseOrderLines] currently carries neither column, so both are projected as
/// NULL. When those columns are added, extend the SELECT below.
/// </summary>
public sealed class PurchaseOrderLineLookup : IPurchaseOrderLineLookup
{
    private readonly QualityDbContext _context;

    public PurchaseOrderLineLookup(QualityDbContext context) => _context = context;

    public async Task<PurchaseOrderLineInfo?> GetAsync(int purchaseOrderLineId, CancellationToken ct)
    {
        var rows = await _context.Database
            .SqlQuery<PurchaseOrderLineInfo>($"""
                SELECT
                    l.[Id]              AS [PurchaseOrderLineId],
                    l.[ItemId]          AS [ItemId],
                    l.[UomId]           AS [UomId],
                    l.[OrderedQuantity] AS [Quantity],
                    CAST(NULL AS int)           AS [ProductTypeId],
                    CAST(NULL AS nvarchar(100)) AS [BatchNo]
                FROM [Procurement].[PurchaseOrderLines] AS l
                WHERE l.[Id] = {purchaseOrderLineId}
                """)
            .ToListAsync(ct);

        return rows.FirstOrDefault();
    }
}
