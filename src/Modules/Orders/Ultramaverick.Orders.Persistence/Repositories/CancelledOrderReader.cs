using Microsoft.EntityFrameworkCore;
using Ultramaverick.Orders.Application.Abstractions;
using Ultramaverick.Orders.Application.Queries.CancelledOrders;

namespace Ultramaverick.Orders.Persistence.Repositories;

public sealed class CancelledOrderReader : ICancelledOrderReader
{
    private readonly OrdersDbContext _context;

    public CancelledOrderReader(OrdersDbContext context) => _context = context;

    public async Task<IReadOnlyList<CancelledOrderDto>> GetByCustomerAsync(int customerId, CancellationToken ct)
        => await _context.CancelledOrders
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CancelledAtUtc)
            .Select(x => new CancelledOrderDto(
                x.Id, x.OrderId, x.CustomerId, x.ReasonId, x.CancelledByUserId, x.CancelledAtUtc))
            .ToListAsync(ct);
}
