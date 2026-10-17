using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Items;

public sealed record GetItemByIdQuery(int ItemId) : IRequest<Result<ItemDto>>;