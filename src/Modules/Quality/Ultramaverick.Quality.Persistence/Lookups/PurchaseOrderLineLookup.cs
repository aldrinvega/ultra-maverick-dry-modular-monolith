using Microsoft.EntityFrameworkCore;
using Ultramaverick.Quality.Application.Abstractions;

namespace Ultramaverick.Quality.Persistence.Lookups;

/// <summary>
/// Reads the Procurement and Catalog schemas directly so Quality can build a QC receipt from
/// a purchase-order line without a project reference to either module. Read-only cross-schema
/// query.
///
/// Deviation: the guide's port returns ProductTypeId and BatchNo, but neither
/// [Procurement].[PurchaseOrderLines] nor [Catalog].[Items] currently carries those columns,
/// so both are projected as NULL. When those columns are added, extend the SELECT below.
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
