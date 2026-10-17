using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.LotNames;

public sealed record GetLotNameByIdQuery(int LotNameId) : IRequest<Result<LotNameDto>>;