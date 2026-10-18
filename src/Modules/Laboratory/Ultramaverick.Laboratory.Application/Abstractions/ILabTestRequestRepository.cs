using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.Application.Abstractions;

public interface ILabTestRequestRepository
{
    Task<LabTestRequest?> GetByIdAsync(int labTestRequestId, CancellationToken ct);
    Task AddAsync(LabTestRequest request, CancellationToken ct);
}
