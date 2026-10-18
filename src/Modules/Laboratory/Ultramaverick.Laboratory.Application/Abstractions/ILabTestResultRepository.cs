using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.Application.Abstractions;

public interface ILabTestResultRepository
{
    Task AddAsync(LabTestResult result, CancellationToken ct);
}
