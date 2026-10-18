using Microsoft.EntityFrameworkCore;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Persistence.Repositories;

public sealed class WarehouseReceiptRepository : IWarehouseReceiptRepository
{
    private readonly WarehouseDbContext _context;

    public WarehouseReceiptRepository(WarehouseDbContext context) => _context = context;

    public Task<bool> ExistsForQcReceiptAsync(int qcReceiptId, CancellationToken ct)
        => _context.WarehouseReceipts.AnyAsync(x => x.QcReceiptId == qcReceiptId, ct);

    public Task<WarehouseReceipt?> GetByIdAsync(int warehouseReceiptId, CancellationToken ct)
        => _context.WarehouseReceipts.FirstOrDefaultAsync(x => x.Id == warehouseReceiptId, ct);

    public async Task<IReadOnlyList<WarehouseReceipt>> ListAsync(
        int? warehouseId, int skip, int take, CancellationToken ct)
        => await _context.WarehouseReceipts
            .AsNoTracking()
            .Where(x => warehouseId == null || x.WarehouseId == warehouseId)
            .OrderByDescending(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

    public Task<int> CountAsync(int? warehouseId, CancellationToken ct)
        => _context.WarehouseReceipts.CountAsync(x => warehouseId == null || x.WarehouseId == warehouseId, ct);

    public async Task AddAsync(WarehouseReceipt receipt, CancellationToken ct)
        => await _context.WarehouseReceipts.AddAsync(receipt, ct);
}
