using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.LotNames;

public sealed record CreateLotNameCommand(string Code, int LotCategoryId, string? SectionName)
    : IRequest<Result<int>>;