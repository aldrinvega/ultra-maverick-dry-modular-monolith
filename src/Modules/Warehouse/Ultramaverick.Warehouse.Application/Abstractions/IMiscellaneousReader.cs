using Ultramaverick.Warehouse.Application.Models;
using Ultramaverick.Warehouse.Application.Queries.Miscellaneous;

namespace Ultramaverick.Warehouse.Application.Abstractions;

public interface IMiscellaneousReader
{
    Task<PagedResult<MiscellaneousReceiptDto>> GetReceiptsPagedAsync(int page, int pageSize, CancellationToken ct);
    Task<PagedResult<MiscellaneousIssueDto>> GetIssuesPagedAsync(int page, int pageSize, CancellationToken ct);
}
