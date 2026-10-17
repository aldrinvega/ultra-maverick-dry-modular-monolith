using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Lookups;

public sealed record GetProductTypeByIdQuery(int ProductTypeId) : IRequest<Result<LookupDto>>;