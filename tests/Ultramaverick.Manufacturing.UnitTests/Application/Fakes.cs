using Ultramaverick.Manufacturing.Application.Abstractions;
using Ultramaverick.Manufacturing.Domain.Entities;

namespace Ultramaverick.Manufacturing.UnitTests.Application;

internal sealed class FakeCurrentUser : ICurrentUser
{
    public int? UserId { get; init; } = 7;
    public string? UserName => "test-user";
    public bool IsAuthenticated => true;
}

internal sealed class FakeTransformationRequestRepository : ITransformationRequestRepository
{
    public TransformationRequest? Stored { get; set; }
    public TransformationRequest? Added { get; private set; }

    public Task<TransformationRequest?> GetByIdAsync(int id, CancellationToken ct)
        => Task.FromResult(Stored);

    public Task AddAsync(TransformationRequest request, CancellationToken ct)
    {
        Added = request;
        return Task.CompletedTask;
    }
}

internal sealed class FakeProductionPlanRepository : IProductionPlanRepository
{
    public bool Exists { get; set; } = true;

    public Task<bool> ExistsAsync(int id, CancellationToken ct) => Task.FromResult(Exists);
}

internal sealed class FakeUnitOfWork : IManufacturingUnitOfWork
{
    public FakeTransformationRequestRepository Requests { get; } = new();
    public FakeProductionPlanRepository Plans { get; } = new();

    public ITransformationRequestRepository TransformationRequests => Requests;
    public IProductionPlanRepository ProductionPlans => Plans;

    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        SaveCount++;
        return Task.FromResult(1);
    }
}
