using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Suppliers;

public sealed record GetSuppliersQuery(int Page, int PageSize, string? Search, bool? IsActive)
    : IRequest<PagedResult<SupplierDto>>;