using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Lookups;

public sealed class UpdateAccountTitleCommandHandler : IRequestHandler<UpdateAccountTitleCommand, Result>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public UpdateAccountTitleCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateAccountTitleCommand request, CancellationToken ct)
    {
        var entity = await _uow.AccountTitles.GetByIdAsync(request.AccountTitleId, ct);
        if (entity is null) return Result.Failure("Account title not found.");

        var name = (request.Name ?? string.Empty).Trim();
        var code = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim();

        var clash = await _uow.AccountTitles.GetByNameAsync(name, ct);
        if (clash is not null && clash.Id != entity.Id)
            return Result.Failure("Account title '" + name + "' already exists.");

        if (code is not null)
        {
            var codeClash = await _uow.AccountTitles.GetByCodeAsync(code, ct);
            if (codeClash is not null && codeClash.Id != entity.Id)
                return Result.Failure("Account title code '" + code + "' already exists.");
        }

        var actorId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required.");

        entity.Rename(name, code, actorId);

        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}