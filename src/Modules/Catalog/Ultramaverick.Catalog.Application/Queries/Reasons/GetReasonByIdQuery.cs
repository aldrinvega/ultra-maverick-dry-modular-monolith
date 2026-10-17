using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Reasons;

public sealed record GetReasonByIdQuery(int ReasonId) : IRequest<Result<ReasonDto>>;