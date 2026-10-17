using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Uoms;

public sealed record GetUomByIdQuery(int UomId) : IRequest<Result<UomDto>>;