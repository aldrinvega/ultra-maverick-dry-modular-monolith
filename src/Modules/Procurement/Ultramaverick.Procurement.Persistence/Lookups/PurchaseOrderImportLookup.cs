using Microsoft.EntityFrameworkCore;
using Ultramaverick.Procurement.Application.Abstractions;

namespace Ultramaverick.Procurement.Persistence.Lookups;

/// <summary>
/// Resolves the codes carried by the legacy PO-summary workbook through the Catalog schema.
/// Read-only cross-schema queries, following the same pattern as <see cref="SupplierLookup"/>.
/// </summary>
public sealed class PurchaseOrderImportLookup : IPurchaseOrderImportLookup
{
    private readonly ProcurementDbContext _context;

    public PurchaseOrderImportLookup(ProcurementDbContext context) => _context = context;

    public async Task<int?> FindSupplierIdByNameAsync(string supplierName, CancellationToken ct)
    {
        var id = await _context.Database
            .SqlQuery<int>($"""
                SELECT TOP 1 [Id] AS [Value]
                FROM [Catalog].[Suppliers]
                WHERE [SupplierName] = {supplierName} AND [IsActive] = 1
                """)
            .FirstOrDefaultAsync(ct);

        return id == 0 ? null : id;
    }

    public async Task<ImportedItem?> FindItemByCodeAsync(string itemCode, CancellationToken ct)
    {
        var rows = await _context.Database
            .SqlQuery<ImportedItem>($"""
                SELECT
                    [Id]              AS [Id],
                    [ItemDescription] AS [Description]
                FROM [Catalog].[Items]
                WHERE [ItemCode] = {itemCode} AND [IsActive] = 1
                """)
            .ToListAsync(ct);

        return rows.FirstOrDefault();
    }

    public async Task<int?> FindUomIdByCodeAsync(string uomCode, CancellationToken ct)
    {
        var id = await _context.Database
            .SqlQuery<int>($"""
                SELECT TOP 1 [Id] AS [Value]
                FROM [Catalog].[Uoms]
                WHERE [Code] = {uomCode} AND [IsActive] = 1
                """)
            .FirstOrDefaultAsync(ct);

        return id == 0 ? null : id;
    }
}
