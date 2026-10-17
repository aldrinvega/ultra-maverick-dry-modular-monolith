using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Items;

public sealed record SetItemActiveCommand(int ItemId, bool IsActive) : IRequest<Result>;