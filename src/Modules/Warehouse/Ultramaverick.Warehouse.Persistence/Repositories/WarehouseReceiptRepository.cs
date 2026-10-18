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
        int skip, int take, CancellationToken ct)
        => await _context.WarehouseReceipts
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

    public Task<int> CountAsync(CancellationToken ct)
        => _context.WarehouseReceipts.CountAsync(ct);

    public async Task AddAsync(WarehouseReceipt receipt, CancellationToken ct)
        => await _context.WarehouseReceipts.AddAsync(receipt, ct);
}
