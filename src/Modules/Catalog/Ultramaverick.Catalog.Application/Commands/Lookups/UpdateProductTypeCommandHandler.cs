using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Lookups;

public sealed class UpdateProductTypeCommandHandler : IRequestHandler<UpdateProductTypeCommand, Result>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public UpdateProductTypeCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateProductTypeCommand request, CancellationToken ct)
    {
        var entity = await _uow.ProductTypes.GetByIdAsync(request.ProductTypeId, ct);
        if (entity is null) return Result.Failure("Product type not found.");

        var name = (request.Name ?? string.Empty).Trim();
        var code = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim();

        var clash = await _uow.ProductTypes.GetByNameAsync(name, ct);
        if (clash is not null && clash.Id != entity.Id)
            return Result.Failure("Product type '" + name + "' already exists.");

        if (code is not null)
        {
            var codeClash = await _uow.ProductTypes.GetByCodeAsync(code, ct);
            if (codeClash is not null && codeClash.Id != entity.Id)
                return Result.Failure("Product type code '" + code + "' already exists.");
        }

        var actorId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required.");

        entity.Rename(name, code, actorId);

        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}