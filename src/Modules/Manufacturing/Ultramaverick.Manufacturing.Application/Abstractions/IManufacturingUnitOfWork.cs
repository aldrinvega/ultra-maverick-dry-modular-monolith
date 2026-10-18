namespace Ultramaverick.Manufacturing.Application.Abstractions;

public interface IManufacturingUnitOfWork
{
    ITransformationRequestRepository TransformationRequests { get; }
    IProductionPlanRepository ProductionPlans { get; }
    Task<int> SaveChangesAsync(CancellationToken ct);
}
