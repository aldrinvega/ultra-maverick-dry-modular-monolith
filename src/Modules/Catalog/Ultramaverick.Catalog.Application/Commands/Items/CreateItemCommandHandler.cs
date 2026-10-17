using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Commands.Items;

public sealed class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, Result<int>>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public CreateItemCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateItemCommand request, CancellationToken ct)
    {
        var code = (request.ItemCode ?? string.Empty).Trim();

        if (await _uow.Items.GetByCodeAsync(code, ct) is not null)
            return Result<int>.Failure($"Item code '{code}' already exists.");
        if (await _uow.ItemCategories.GetByIdAsync(request.ItemCategoryId, ct) is null)
            return Result<int>.Failure("Item category does not exist.");
        if (await _uow.Uoms.GetByIdAsync(request.UomId, ct) is null)
            return Result<int>.Failure("UOM does not exist.");

        var item = Item.Create(
            code, request.ItemDescription, request.ItemCategoryId, request.UomId,
            request.BufferLevel, request.IsExpirable, _currentUser.UserId);

        await _uow.Items.AddAsync(item, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<int>.Success(item.Id);
    }
}