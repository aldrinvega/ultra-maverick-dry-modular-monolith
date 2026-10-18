using Microsoft.EntityFrameworkCore;
using Ultramaverick.Manufacturing.Application.Abstractions;
using Ultramaverick.Manufacturing.Domain.Entities;

namespace Ultramaverick.Manufacturing.Persistence.Repositories;

public sealed class TransformationRequestRepository : ITransformationRequestRepository
{
    private readonly ManufacturingDbContext _context;

    public TransformationRequestRepository(ManufacturingDbContext context) => _context = context;

    public Task<TransformationRequest?> GetByIdAsync(int id, CancellationToken ct)
        => _context.TransformationRequests
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(TransformationRequest request, CancellationToken ct)
        => await _context.TransformationRequests.AddAsync(request, ct);
}
