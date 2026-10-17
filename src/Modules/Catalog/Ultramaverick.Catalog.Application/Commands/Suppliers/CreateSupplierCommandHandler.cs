using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Commands.Suppliers;

public sealed class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, Result<int>>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public CreateSupplierCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateSupplierCommand request, CancellationToken ct)
    {
        var code = (request.SupplierCode ?? string.Empty).Trim();

        if (await _uow.Suppliers.GetByCodeAsync(code, ct) is not null)
            return Result<int>.Failure($"Supplier code '{code}' already exists.");

        var supplier = Supplier.Create(code, request.SupplierName, request.SupplierAddress, _currentUser.UserId);

        await _uow.Suppliers.AddAsync(supplier, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<int>.Success(supplier.Id);
    }
}