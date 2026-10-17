using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Uoms;

public sealed class SetUomActiveCommandHandler : IRequestHandler<SetUomActiveCommand, Result>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public SetUomActiveCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(SetUomActiveCommand request, CancellationToken ct)
    {
        var uom = await _uow.Uoms.GetByIdAsync(request.UomId, ct);
        if (uom is null) return Result.Failure("UOM not found.");

        var actorId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required.");

        uom.SetActive(request.IsActive, actorId);

        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}