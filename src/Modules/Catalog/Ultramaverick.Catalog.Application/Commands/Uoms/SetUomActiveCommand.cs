using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Uoms;

public sealed record SetUomActiveCommand(int UomId, bool IsActive) : IRequest<Result>;