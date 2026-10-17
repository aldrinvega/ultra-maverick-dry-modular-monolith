using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Lookups;

public sealed record UpdateProductTypeCommand(int ProductTypeId, string Name, string? Code) : IRequest<Result>;