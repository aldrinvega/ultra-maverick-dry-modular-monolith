using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.LotNames;

public sealed class UpdateLotNameCommandHandler : IRequestHandler<UpdateLotNameCommand, Result>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public UpdateLotNameCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateLotNameCommand request, CancellationToken ct)
    {
        var lotName = await _uow.LotNames.GetByIdAsync(request.LotNameId, ct);
        if (lotName is null) return Result.Failure("Lot name not found.");

        var code = (request.Code ?? string.Empty).Trim();
        var clash = await _uow.LotNames.GetByCodeAsync(code, ct);
        if (clash is not null && clash.Id != lotName.Id)
            return Result.Failure($"Lot name code '{code}' already exists.");

        if (await _uow.LotCategories.GetByIdAsync(request.LotCategoryId, ct) is null)
            return Result.Failure("Lot category does not exist.");

        var actorId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required.");

        lotName.Update(code, request.LotCategoryId, request.SectionName, actorId);

        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}