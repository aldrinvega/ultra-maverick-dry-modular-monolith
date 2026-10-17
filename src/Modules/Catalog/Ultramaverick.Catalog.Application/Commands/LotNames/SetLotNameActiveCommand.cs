using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.LotNames;

public sealed record SetLotNameActiveCommand(int LotNameId, bool IsActive) : IRequest<Result>;