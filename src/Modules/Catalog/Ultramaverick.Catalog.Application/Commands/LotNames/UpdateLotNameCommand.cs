using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.LotNames;

public sealed record UpdateLotNameCommand(int LotNameId, string Code, int LotCategoryId, string? SectionName)
    : IRequest<Result>;