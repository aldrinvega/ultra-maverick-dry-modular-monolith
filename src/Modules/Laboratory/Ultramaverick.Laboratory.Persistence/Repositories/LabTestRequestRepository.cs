using Microsoft.EntityFrameworkCore;
using Ultramaverick.Laboratory.Application.Abstractions;
using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.Persistence.Repositories;

public sealed class LabTestRequestRepository : ILabTestRequestRepository
{
    private readonly LaboratoryDbContext _context;

    public LabTestRequestRepository(LaboratoryDbContext context) => _context = context;

    // Tracked so AcceptLabReceiveRequest can read the assigned id and RecordLabResult can
    // read WarehouseReceiptId/BatchNo from the same transaction boundary.
    public Task<LabTestRequest?> GetByIdAsync(int labTestRequestId, CancellationToken ct)
        => _context.LabTestRequests.FirstOrDefaultAsync(x => x.Id == labTestRequestId, ct);

    public async Task AddAsync(LabTestRequest entity, CancellationToken ct)
        => await _context.LabTestRequests.AddAsync(entity, ct);
}
