using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Uoms;

public sealed class UpdateUomCommandHandler : IRequestHandler<UpdateUomCommand, Result>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public UpdateUomCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateUomCommand request, CancellationToken ct)
    {
        var uom = await _uow.Uoms.GetByIdAsync(request.UomId, ct);
        if (uom is null) return Result.Failure("UOM not found.");

        var code = (request.Code ?? string.Empty).Trim();
        var clash = await _uow.Uoms.GetByCodeAsync(code, ct);
        if (clash is not null && clash.Id != uom.Id)
            return Result.Failure($"UOM code '{code}' already exists.");

        var actorId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required.");

        uom.Update(code, request.Description, actorId);

        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}