using Microsoft.EntityFrameworkCore;
using Ultramaverick.Orders.Application.Abstractions;
using Ultramaverick.Orders.Application.Models;
using Ultramaverick.Orders.Application.Queries.CancelledOrders;
using Ultramaverick.Orders.Persistence;

namespace Ultramaverick.Orders.Persistence.Repositories;

public sealed class CancelledOrderReader : ICancelledOrderReader
{
    private readonly OrdersDbContext _context;

    public CancelledOrderReader(OrdersDbContext context) => _context = context;

    public async Task<IReadOnlyList<CancelledOrderDto>> GetByCustomerAsync(int customerId, CancellationToken ct)
        => await Projected(_context.CancelledOrders
                .AsNoTracking()
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.CancelledAtUtc)
                .ThenByDescending(x => x.Id)
                .Take(1000))
            .ToListAsync(ct);

    public async Task<PagedResult<CancelledOrderDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct)
    {
        var source = _context.CancelledOrders.AsNoTracking();
        var totalCount = await source.CountAsync(ct);

        var items = await Projected(source
                .OrderByDescending(x => x.CancelledAtUtc)
                .ThenByDescending(x => x.Id))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<CancelledOrderDto>(items, page, pageSize, totalCount);
    }

    private static IQueryable<CancelledOrderDto> Projected(IQueryable<Domain.Entities.CancelledOrder> source)
        => source.Select(x => new CancelledOrderDto(
            x.Id, x.OrderId, x.CustomerId, x.ReasonId, x.CancelledByUserId, x.CancelledAtUtc));
}
