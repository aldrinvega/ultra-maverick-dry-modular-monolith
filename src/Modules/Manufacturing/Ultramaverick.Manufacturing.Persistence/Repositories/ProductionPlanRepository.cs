using Microsoft.EntityFrameworkCore;
using Ultramaverick.Manufacturing.Application.Abstractions;

namespace Ultramaverick.Manufacturing.Persistence.Repositories;

public sealed class ProductionPlanRepository : IProductionPlanRepository
{
    private readonly ManufacturingDbContext _context;

    public ProductionPlanRepository(ManufacturingDbContext context) => _context = context;

    public Task<bool> ExistsAsync(int id, CancellationToken ct)
        => _context.ProductionPlans.AnyAsync(x => x.Id == id, ct);
}
