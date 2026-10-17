using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Lookups;

public sealed class SetProductTypeActiveCommandHandler : IRequestHandler<SetProductTypeActiveCommand, Result>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public SetProductTypeActiveCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(SetProductTypeActiveCommand request, CancellationToken ct)
    {
        var entity = await _uow.ProductTypes.GetByIdAsync(request.ProductTypeId, ct);
        if (entity is null) return Result.Failure("Product type not found.");

        var actorId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required.");

        entity.SetActive(request.IsActive, actorId);

        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}