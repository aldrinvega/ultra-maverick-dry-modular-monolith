namespace Ultramaverick.Laboratory.Application.Abstractions;

/// <summary>Aggregates every Laboratory repository behind one transaction boundary.</summary>
public interface ILaboratoryUnitOfWork
{
    ILabTestRequestRepository LabTestRequests { get; }
    ILabReceiveRequestRepository LabReceiveRequests { get; }
    ILabTestResultRepository LabTestResults { get; }

    Task<int> SaveChangesAsync(CancellationToken ct);
}
