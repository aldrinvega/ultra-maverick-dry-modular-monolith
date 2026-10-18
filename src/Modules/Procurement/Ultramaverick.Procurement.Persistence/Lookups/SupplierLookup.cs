using Microsoft.EntityFrameworkCore;
using Ultramaverick.Procurement.Application.Abstractions;

namespace Ultramaverick.Procurement.Persistence.Lookups;

/// <summary>
/// Reads the Catalog schema directly so Procurement can validate a supplier without a
/// project reference to Catalog. This is a read-only cross-schema query.
/// </summary>
public sealed class SupplierLookup : ISupplierLookup
{
    private readonly ProcurementDbContext _context;

    public SupplierLookup(ProcurementDbContext context) => _context = context;

    public Task<bool> ExistsAsync(int supplierId, CancellationToken ct)
        => _context.Database
            .SqlQuery<bool>($"SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM [Catalog].[Suppliers] WHERE [Id] = {supplierId} AND [IsActive] = 1) THEN 1 ELSE 0 END AS BIT) AS [Value]")
            .SingleAsync(ct);
}
