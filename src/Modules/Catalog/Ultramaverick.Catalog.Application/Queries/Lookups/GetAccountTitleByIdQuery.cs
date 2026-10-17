using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Lookups;

public sealed record GetAccountTitleByIdQuery(int AccountTitleId) : IRequest<Result<LookupDto>>;