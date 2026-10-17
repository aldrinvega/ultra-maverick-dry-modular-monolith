using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Reasons;

public sealed record CreateReasonCommand(string Name, int? MenuId) : IRequest<Result<int>>;