using Microsoft.EntityFrameworkCore;
using Ultramaverick.Reporting.Application.Abstractions;
using Ultramaverick.Reporting.Domain.Entities;

namespace Ultramaverick.Reporting.Persistence.Repositories;

public sealed class OrderFulfillmentStore : IOrderFulfillmentStore
{
    private readonly ReportingDbContext _context;

    public OrderFulfillmentStore(ReportingDbContext context) => _context = context;

    public Task<OrderFulfillment?> FindAsync(string orderNumber, CancellationToken ct)
        => _context.OrderFulfillment.FirstOrDefaultAsync(x => x.OrderNumber == orderNumber, ct);

    public async Task AddAsync(OrderFulfillment fulfillment, CancellationToken ct)
        => await _context.OrderFulfillment.AddAsync(fulfillment, ct);
}
