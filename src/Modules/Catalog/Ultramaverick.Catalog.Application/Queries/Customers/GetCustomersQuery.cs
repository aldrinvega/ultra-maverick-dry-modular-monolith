using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Customers;

public sealed record GetCustomersQuery(int Page, int PageSize, string? Search, bool? IsActive)
    : IRequest<PagedResult<CustomerDto>>;