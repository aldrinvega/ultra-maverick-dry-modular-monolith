using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Items;

public sealed class UpdateItemCommandHandler : IRequestHandler<UpdateItemCommand, Result>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public UpdateItemCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateItemCommand request, CancellationToken ct)
    {
        var item = await _uow.Items.GetByIdAsync(request.ItemId, ct);
        if (item is null) return Result.Failure("Item not found.");

        if (await _uow.ItemCategories.GetByIdAsync(request.ItemCategoryId, ct) is null)
            return Result.Failure("Item category does not exist.");
        if (await _uow.Uoms.GetByIdAsync(request.UomId, ct) is null)
            return Result.Failure("UOM does not exist.");

        var actorId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required.");

        item.Update(request.ItemDescription, request.ItemCategoryId, request.UomId,
            request.BufferLevel, request.IsExpirable, actorId);

        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}