using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Commands.Lookups;

public sealed class CreateProductTypeCommandHandler : IRequestHandler<CreateProductTypeCommand, Result<int>>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public CreateProductTypeCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateProductTypeCommand request, CancellationToken ct)
    {
        var name = (request.Name ?? string.Empty).Trim();
        var code = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim();

        if (await _uow.ProductTypes.GetByNameAsync(name, ct) is not null)
            return Result<int>.Failure("Product type '" + name + "' already exists.");

        if (code is not null && await _uow.ProductTypes.GetByCodeAsync(code, ct) is not null)
            return Result<int>.Failure("Product type code '" + code + "' already exists.");

        var entity = ProductType.Create(name, _currentUser.UserId, code);

        await _uow.ProductTypes.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<int>.Success(entity.Id);
    }
}