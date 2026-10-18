using Ultramaverick.Laboratory.Application.Abstractions;
using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.Persistence.Repositories;

public sealed class LabTestResultRepository : ILabTestResultRepository
{
    private readonly LaboratoryDbContext _context;

    public LabTestResultRepository(LaboratoryDbContext context) => _context = context;

    public async Task AddAsync(LabTestResult entity, CancellationToken ct)
        => await _context.LabTestResults.AddAsync(entity, ct);
}
