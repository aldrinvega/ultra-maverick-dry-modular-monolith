using Microsoft.EntityFrameworkCore;
using Ultramaverick.Manufacturing.Application.Abstractions;
using Ultramaverick.Manufacturing.Application.Models;
using Ultramaverick.Manufacturing.Application.Queries.TransformationRequests;

namespace Ultramaverick.Manufacturing.Persistence.Repositories;

public sealed class TransformationRequestReader : ITransformationRequestReader
{
    private readonly ManufacturingDbContext _context;

    public TransformationRequestReader(ManufacturingDbContext context) => _context = context;

    public async Task<PagedResult<TransformationRequestSummaryDto>> GetPagedAsync(
        string? status, int page, int pageSize, CancellationToken ct)
    {
        var source = _context.TransformationRequests.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
            source = source.Where(x => x.Status == status);

        var totalCount = await source.CountAsync(ct);

        var items = await source
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TransformationRequestSummaryDto(
                x.Id, x.ProductionPlanId, x.WarehouseId, x.Status, x.IsActive,
                x.Lines.Count, x.CreatedAtUtc))
            .ToListAsync(ct);

        return new PagedResult<TransformationRequestSummaryDto>(items, page, pageSize, totalCount);
    }

    public async Task<TransformationRequestDetailDto?> GetByIdAsync(int id, CancellationToken ct)
        => await _context.TransformationRequests
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new TransformationRequestDetailDto(
                x.Id,
                x.ProductionPlanId,
                x.WarehouseId,
                x.Status,
                x.IsActive,
                x.CreatedAtUtc,
                x.Lines
                    .OrderBy(l => l.Id)
                    .Select(l => new TransformationRequestLineDto(l.Id, l.ItemId, l.RequiredQuantity))
                    .ToList(),
                x.Preparations
                    .OrderBy(p => p.Id)
                    .Select(p => new TransformationRequestPreparationDto(
                        p.Id, p.PreparedByUserId, p.PreparedAtUtc, p.QuantityNeeded))
                    .ToList(),
                x.Rejects
                    .OrderBy(r => r.Id)
                    .Select(r => new TransformationRequestRejectDto(
                        r.Id, r.ReasonId, r.Quantity, r.RejectedAtUtc))
                    .ToList()))
            .FirstOrDefaultAsync(ct);
}
