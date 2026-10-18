using Ultramaverick.Laboratory.Application.Abstractions;

namespace Ultramaverick.Laboratory.Persistence.Repositories;

public sealed class LaboratoryUnitOfWork : ILaboratoryUnitOfWork
{
    private readonly LaboratoryDbContext _context;

    public LaboratoryUnitOfWork(LaboratoryDbContext context)
    {
        _context = context;

        LabTestRequests = new LabTestRequestRepository(context);
        LabReceiveRequests = new LabReceiveRequestRepository(context);
        LabTestResults = new LabTestResultRepository(context);
    }

    public ILabTestRequestRepository LabTestRequests { get; }
    public ILabReceiveRequestRepository LabReceiveRequests { get; }
    public ILabTestResultRepository LabTestResults { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}
