using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Uoms;

public sealed record UpdateUomCommand(int UomId, string Code, string Description) : IRequest<Result>;