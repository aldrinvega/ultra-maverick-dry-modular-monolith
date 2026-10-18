using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.Application.Abstractions;

public interface ILabReceiveRequestRepository
{
    Task<LabReceiveRequest?> GetByIdAsync(int labReceiveRequestId, CancellationToken ct);
    Task AddAsync(LabReceiveRequest request, CancellationToken ct);
}
