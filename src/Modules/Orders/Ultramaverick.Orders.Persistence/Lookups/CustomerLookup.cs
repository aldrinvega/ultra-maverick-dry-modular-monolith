using Microsoft.EntityFrameworkCore;
using Ultramaverick.Orders.Application.Abstractions;

namespace Ultramaverick.Orders.Persistence.Lookups;

/// <summary>
/// Reads the Catalog schema directly so Orders can validate a customer without a
/// project reference to Catalog. This is a read-only cross-schema query.
/// </summary>
public sealed class CustomerLookup : ICustomerLookup
{
    private readonly OrdersDbContext _context;

    public CustomerLookup(OrdersDbContext context) => _context = context;

    public Task<bool> ExistsAsync(int customerId, CancellationToken ct)
        => _context.Database
            .SqlQuery<bool>($"SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM [Catalog].[Customers] WHERE [Id] = {customerId} AND [IsActive] = 1) THEN 1 ELSE 0 END AS BIT) AS [Value]")
            .SingleAsync(ct);
}
