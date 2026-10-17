using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Lookups;

public sealed class SetAccountTitleActiveCommandHandler : IRequestHandler<SetAccountTitleActiveCommand, Result>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public SetAccountTitleActiveCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(SetAccountTitleActiveCommand request, CancellationToken ct)
    {
        var entity = await _uow.AccountTitles.GetByIdAsync(request.AccountTitleId, ct);
        if (entity is null) return Result.Failure("Account title not found.");

        var actorId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required.");

        entity.SetActive(request.IsActive, actorId);

        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}