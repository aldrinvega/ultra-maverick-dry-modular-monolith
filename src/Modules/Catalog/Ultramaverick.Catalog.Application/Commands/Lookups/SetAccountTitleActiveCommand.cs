using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Lookups;

public sealed record SetAccountTitleActiveCommand(int AccountTitleId, bool IsActive) : IRequest<Result>;