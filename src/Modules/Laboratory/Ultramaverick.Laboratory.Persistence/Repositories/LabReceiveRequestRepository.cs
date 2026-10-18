using Microsoft.EntityFrameworkCore;
using Ultramaverick.Laboratory.Application.Abstractions;
using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.Persistence.Repositories;

public sealed class LabReceiveRequestRepository : ILabReceiveRequestRepository
{
    private readonly LaboratoryDbContext _context;

    public LabReceiveRequestRepository(LaboratoryDbContext context) => _context = context;

    public Task<LabReceiveRequest?> GetByIdAsync(int labReceiveRequestId, CancellationToken ct)
        => _context.LabReceiveRequests.FirstOrDefaultAsync(x => x.Id == labReceiveRequestId, ct);

    public async Task AddAsync(LabReceiveRequest entity, CancellationToken ct)
        => await _context.LabReceiveRequests.AddAsync(entity, ct);
}
