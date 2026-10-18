using Microsoft.EntityFrameworkCore;
using Ultramaverick.Warehouse.Application.Abstractions;
using WarehouseEntity = Ultramaverick.Warehouse.Domain.Entities.Warehouse;

namespace Ultramaverick.Warehouse.Persistence.Repositories;

public sealed class WarehouseRepository : IWarehouseRepository
{
    private readonly WarehouseDbContext _context;

    public WarehouseRepository(WarehouseDbContext context) => _context = context;

    public Task<WarehouseEntity?> GetByIdAsync(int warehouseId, CancellationToken ct)
        => _context.Warehouses.FirstOrDefaultAsync(x => x.Id == warehouseId, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct)
        => _context.Warehouses.AnyAsync(x => x.Code == code, ct);

    public async Task<IReadOnlyList<WarehouseEntity>> ListAsync(bool? isActive, CancellationToken ct)
        => await _context.Warehouses
            .AsNoTracking()
            .Where(x => isActive == null || x.IsActive == isActive)
            .OrderBy(x => x.Id)
            .ToListAsync(ct);

    public async Task AddAsync(WarehouseEntity warehouse, CancellationToken ct)
        => await _context.Warehouses.AddAsync(warehouse, ct);
}
