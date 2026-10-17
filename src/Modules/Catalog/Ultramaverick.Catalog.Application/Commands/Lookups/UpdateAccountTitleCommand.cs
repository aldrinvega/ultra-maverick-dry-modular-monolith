using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Lookups;

public sealed record UpdateAccountTitleCommand(int AccountTitleId, string Name, string? Code) : IRequest<Result>;