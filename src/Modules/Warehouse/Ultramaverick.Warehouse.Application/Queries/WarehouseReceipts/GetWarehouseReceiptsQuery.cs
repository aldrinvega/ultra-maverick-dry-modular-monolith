using MediatR;
using Ultramaverick.Warehouse.Application.Models;

namespace Ultramaverick.Warehouse.Application.Queries.WarehouseReceipts;

public sealed record GetWarehouseReceiptsQuery(int Page, int PageSize)
    : IRequest<PagedResult<WarehouseReceiptDto>>;
