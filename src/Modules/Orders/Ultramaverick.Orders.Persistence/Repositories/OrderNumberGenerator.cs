using Microsoft.EntityFrameworkCore;
using Ultramaverick.Orders.Application.Abstractions;

namespace Ultramaverick.Orders.Persistence.Repositories;

public sealed class OrderNumberGenerator : IOrderNumberGenerator
{
    private readonly OrdersDbContext _context;

    public OrderNumberGenerator(OrdersDbContext context) => _context = context;

    public async Task<string> NextAsync(DateTime utcNow, CancellationToken ct)
    {
        var prefix = $"SO-{utcNow:yyyyMMdd}-";

        var last = await _context.Orders
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber.Length)
            .ThenByDescending(o => o.OrderNumber)
            .Select(o => o.OrderNumber)
            .FirstOrDefaultAsync(ct);

        var next = (last is not null && int.TryParse(last.AsSpan(prefix.Length), out var n)) ? n + 1 : 1;
        return $"{prefix}{next:D4}";
    }
}
