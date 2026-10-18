using Microsoft.EntityFrameworkCore;
using Ultramaverick.Orders.Application.Abstractions;
using Ultramaverick.Orders.Application.Models;
using Ultramaverick.Orders.Application.Queries.Orders;

namespace Ultramaverick.Orders.Persistence.Repositories;

public sealed class OrderReader : IOrderReader
{
    private readonly OrdersDbContext _context;

    public OrderReader(OrdersDbContext context) => _context = context;

    public async Task<PagedResult<OrderSummaryDto>> GetPagedAsync(
        int? customerId, string? status, int page, int pageSize, CancellationToken ct)
    {
        var source = _context.Orders.AsNoTracking();

        if (customerId is not null)
            source = source.Where(x => x.CustomerId == customerId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            source = source.Where(x => x.Status == status);

        var totalCount = await source.CountAsync(ct);

        var items = await source
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new OrderSummaryDto(
                x.Id, x.OrderNumber, x.CustomerId, x.Status, x.OrderDate, x.DeliveryDate, x.IsActive))
            .ToListAsync(ct);

        return new PagedResult<OrderSummaryDto>(items, page, pageSize, totalCount);
    }

    public async Task<OrderDetailDto?> GetByIdAsync(int orderId, CancellationToken ct)
        => await _context.Orders
            .AsNoTracking()
            .Where(x => x.Id == orderId)
            .Select(x => new OrderDetailDto(
                x.Id,
                x.OrderNumber,
                x.CustomerId,
                x.CustomerPosition,
                x.FarmId,
                x.OrderDate,
                x.DeliveryDate,
                x.Status,
                x.Remarks,
                x.IsActive,
                x.Lines
                    .OrderBy(l => l.Id)
                    .Select(l => new OrderLineDto(
                        l.Id, l.ItemId, l.UomId, l.QuantityOrdered, l.AllocatedQuantity,
                        l.PlateNumber, l.DeliveryStatus, l.IsActive))
                    .ToList()))
            .FirstOrDefaultAsync(ct);

    public async Task<PagedResult<MoveOrderDto>> GetMoveOrdersPagedAsync(
        int page, int pageSize, CancellationToken ct)
    {
        var source = _context.MoveOrders.AsNoTracking();
        var totalCount = await source.CountAsync(ct);

        var items = await source
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MoveOrderDto(
                x.Id, x.OrderNo, x.ItemId, x.QuantityOrdered, x.Status, x.OrderDate, x.DateNeeded, x.IsActive))
            .ToListAsync(ct);

        return new PagedResult<MoveOrderDto>(items, page, pageSize, totalCount);
    }

    public async Task<PagedResult<TransactedMoveOrderDto>> GetTransactedPagedAsync(
        int page, int pageSize, CancellationToken ct)
    {
        var source = _context.TransactedMoveOrders.AsNoTracking();
        var totalCount = await source.CountAsync(ct);

        var items = await source
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TransactedMoveOrderDto(
                x.Id, x.MoveOrderId, x.OrderNo, x.DeliveryDate, x.IsTransact))
            .ToListAsync(ct);

        return new PagedResult<TransactedMoveOrderDto>(items, page, pageSize, totalCount);
    }
}
