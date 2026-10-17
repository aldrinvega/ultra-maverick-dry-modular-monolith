using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Customers;

public sealed record GetCustomerByIdQuery(int CustomerId) : IRequest<Result<CustomerDto>>;