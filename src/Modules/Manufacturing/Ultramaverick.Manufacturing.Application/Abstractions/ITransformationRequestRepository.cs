using Ultramaverick.Manufacturing.Domain.Entities;

namespace Ultramaverick.Manufacturing.Application.Abstractions;

public interface ITransformationRequestRepository
{
    Task<TransformationRequest?> GetByIdAsync(int id, CancellationToken ct);
    Task AddAsync(TransformationRequest request, CancellationToken ct);
}
