using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Reasons;

public sealed record SetReasonActiveCommand(int ReasonId, bool IsActive) : IRequest<Result>;