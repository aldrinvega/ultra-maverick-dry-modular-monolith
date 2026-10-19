using Microsoft.EntityFrameworkCore;
using Ultramaverick.Laboratory.Application.Abstractions;
using Ultramaverick.Laboratory.Application.Models;
using Ultramaverick.Laboratory.Application.Queries.LabTestRequests;

namespace Ultramaverick.Laboratory.Persistence.Repositories;

public sealed class LabTestRequestReader : ILabTestRequestReader
{
    private readonly LaboratoryDbContext _context;

    public LabTestRequestReader(LaboratoryDbContext context) => _context = context;

    public async Task<PagedResult<LabTestRequestDto>> GetPagedAsync(
        string? status, int page, int pageSize, CancellationToken ct)
    {
        var source = _context.LabTestRequests.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
            source = source.Where(x => x.Status == status);

        var totalCount = await source.CountAsync(ct);

        var items = await source
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new LabTestRequestDto(
                x.Id, x.WarehouseReceiptId, x.BatchNo, x.Quantity, x.TestType,
                x.DateRequested, x.DateNeeded, x.Status, x.Remarks, x.RequestedByUserId, x.IsActive))
            .ToListAsync(ct);

        return new PagedResult<LabTestRequestDto>(items, page, pageSize, totalCount);
    }

    public async Task<LabTestRequestDto?> GetByIdAsync(int id, CancellationToken ct)
        => await _context.LabTestRequests
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new LabTestRequestDto(
                x.Id, x.WarehouseReceiptId, x.BatchNo, x.Quantity, x.TestType,
                x.DateRequested, x.DateNeeded, x.Status, x.Remarks, x.RequestedByUserId, x.IsActive))
            .FirstOrDefaultAsync(ct);
}
