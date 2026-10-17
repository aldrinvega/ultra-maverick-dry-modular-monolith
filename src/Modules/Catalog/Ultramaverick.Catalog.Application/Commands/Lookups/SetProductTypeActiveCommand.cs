using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Lookups;

public sealed record SetProductTypeActiveCommand(int ProductTypeId, bool IsActive) : IRequest<Result>;