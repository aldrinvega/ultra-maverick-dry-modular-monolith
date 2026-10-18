using Ultramaverick.Laboratory.Application.Abstractions;
using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.UnitTests.Fakes;

internal sealed class FakeLabTestRequestRepository : ILabTestRequestRepository
{
    private readonly List<LabTestRequest> _requests = new();
    private readonly Dictionary<int, LabTestRequest> _byId = new();

    public IReadOnlyList<LabTestRequest> All => _requests;

    public void Seed(LabTestRequest request) => _requests.Add(request);

    public void SeedWithId(int id, LabTestRequest request)
    {
        SetId(request, id);
        _byId[id] = request;
        _requests.Add(request);
    }

    public Task<LabTestRequest?> GetByIdAsync(int labTestRequestId, CancellationToken ct)
        => Task.FromResult(_byId.TryGetValue(labTestRequestId, out var request) ? request : null);

    public Task AddAsync(LabTestRequest entity, CancellationToken ct)
    {
        _requests.Add(entity);
        return Task.CompletedTask;
    }

    private static void SetId(object entity, int id)
        => entity.GetType().GetProperty("Id")!.SetValue(entity, id);
}

internal sealed class FakeLabReceiveRequestRepository : ILabReceiveRequestRepository
{
    private readonly List<LabReceiveRequest> _requests = new();
    private readonly Dictionary<int, LabReceiveRequest> _byId = new();

    public IReadOnlyList<LabReceiveRequest> All => _requests;

    public void SeedWithId(int id, LabReceiveRequest request)
    {
        SetId(request, id);
        _byId[id] = request;
        _requests.Add(request);
    }

    public Task<LabReceiveRequest?> GetByIdAsync(int labReceiveRequestId, CancellationToken ct)
        => Task.FromResult(_byId.TryGetValue(labReceiveRequestId, out var request) ? request : null);

    public Task AddAsync(LabReceiveRequest entity, CancellationToken ct)
    {
        _requests.Add(entity);
        return Task.CompletedTask;
    }

    private static void SetId(object entity, int id)
        => entity.GetType().GetProperty("Id")!.SetValue(entity, id);
}

internal sealed class FakeLabTestResultRepository : ILabTestResultRepository
{
    private readonly List<LabTestResult> _results = new();

    public IReadOnlyList<LabTestResult> All => _results;

    public Task AddAsync(LabTestResult entity, CancellationToken ct)
    {
        _results.Add(entity);
        return Task.CompletedTask;
    }
}

internal sealed class FakeLaboratoryUnitOfWork : ILaboratoryUnitOfWork
{
    public FakeLabTestRequestRepository LabTestRequestsRepo { get; } = new();
    public FakeLabReceiveRequestRepository LabReceiveRequestsRepo { get; } = new();
    public FakeLabTestResultRepository LabTestResultsRepo { get; } = new();

    public int SaveCount { get; private set; }

    public ILabTestRequestRepository LabTestRequests => LabTestRequestsRepo;
    public ILabReceiveRequestRepository LabReceiveRequests => LabReceiveRequestsRepo;
    public ILabTestResultRepository LabTestResults => LabTestResultsRepo;

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        SaveCount++;
        return Task.FromResult(SaveCount);
    }
}

internal sealed class FakeCurrentUser : ICurrentUser
{
    public int? UserId { get; set; } = 1;
    public string? UserName { get; set; } = "tester";
    public bool IsAuthenticated { get; set; } = true;
}
