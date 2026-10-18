using Ultramaverick.Manufacturing.Application.Abstractions;

namespace Ultramaverick.Manufacturing.Persistence.Repositories;

public sealed class ManufacturingUnitOfWork : IManufacturingUnitOfWork
{
    private readonly ManufacturingDbContext _context;

    public ManufacturingUnitOfWork(ManufacturingDbContext context)
    {
        _context = context;

        TransformationRequests = new TransformationRequestRepository(context);
        ProductionPlans = new ProductionPlanRepository(context);
    }

    public ITransformationRequestRepository TransformationRequests { get; }

    public IProductionPlanRepository ProductionPlans { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}
