using MediatR;
using Ultramaverick.Procurement.Application.Models;

namespace Ultramaverick.Procurement.Application.Queries.PurchaseOrders;

public sealed record GetPurchaseOrdersQuery(int Page, int PageSize, string? Search)
    : IRequest<PagedResult<PurchaseOrderDto>>;
