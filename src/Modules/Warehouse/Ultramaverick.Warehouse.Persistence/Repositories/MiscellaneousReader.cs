using Microsoft.EntityFrameworkCore;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Application.Models;
using Ultramaverick.Warehouse.Application.Queries.Miscellaneous;

namespace Ultramaverick.Warehouse.Persistence.Repositories;

public sealed class MiscellaneousReader : IMiscellaneousReader
{
    private readonly WarehouseDbContext _context;

    public MiscellaneousReader(WarehouseDbContext context) => _context = context;

    public async Task<PagedResult<MiscellaneousReceiptDto>> GetReceiptsPagedAsync(
        int page, int pageSize, CancellationToken ct)
    {
        var source = _context.MiscellaneousReceipts.AsNoTracking();
        var totalCount = await source.CountAsync(ct);

        var items = await source
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MiscellaneousReceiptDto(
                x.Id, x.WarehouseId, x.RefNo, x.ReceivedAtUtc, x.Remarks, x.ReceivedByUserId, x.IsActive))
            .ToListAsync(ct);

        return new PagedResult<MiscellaneousReceiptDto>(items, page, pageSize, totalCount);
    }

    public async Task<PagedResult<MiscellaneousIssueDto>> GetIssuesPagedAsync(
        int page, int pageSize, CancellationToken ct)
    {
        var source = _context.MiscellaneousIssues.AsNoTracking();
        var totalCount = await source.CountAsync(ct);

        var items = await source
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MiscellaneousIssueDto(
                x.Id, x.WarehouseId, x.RefNo, x.IssuedAtUtc, x.Remarks, x.IssuedByUserId, x.IsActive))
            .ToListAsync(ct);

        return new PagedResult<MiscellaneousIssueDto>(items, page, pageSize, totalCount);
    }
}
