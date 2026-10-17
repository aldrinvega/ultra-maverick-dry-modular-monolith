using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Items;

public sealed record CreateItemCommand(
    string ItemCode, string ItemDescription, int ItemCategoryId, int UomId,
    decimal? BufferLevel, bool IsExpirable) : IRequest<Result<int>>;