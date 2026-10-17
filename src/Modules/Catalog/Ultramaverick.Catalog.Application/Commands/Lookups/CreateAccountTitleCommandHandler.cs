using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Commands.Lookups;

public sealed class CreateAccountTitleCommandHandler : IRequestHandler<CreateAccountTitleCommand, Result<int>>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public CreateAccountTitleCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateAccountTitleCommand request, CancellationToken ct)
    {
        var name = (request.Name ?? string.Empty).Trim();
        var code = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim();

        if (await _uow.AccountTitles.GetByNameAsync(name, ct) is not null)
            return Result<int>.Failure("Account title '" + name + "' already exists.");

        if (code is not null && await _uow.AccountTitles.GetByCodeAsync(code, ct) is not null)
            return Result<int>.Failure("Account title code '" + code + "' already exists.");

        var entity = AccountTitle.Create(name, _currentUser.UserId, code);

        await _uow.AccountTitles.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<int>.Success(entity.Id);
    }
}