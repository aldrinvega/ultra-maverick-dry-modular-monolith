using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Suppliers;

public sealed class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, Result>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public UpdateSupplierCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateSupplierCommand request, CancellationToken ct)
    {
        var supplier = await _uow.Suppliers.GetByIdAsync(request.SupplierId, ct);
        if (supplier is null) return Result.Failure("Supplier not found.");

        var code = (request.SupplierCode ?? string.Empty).Trim();
        var clash = await _uow.Suppliers.GetByCodeAsync(code, ct);
        if (clash is not null && clash.Id != supplier.Id)
            return Result.Failure($"Supplier code '{code}' already exists.");

        var actorId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required.");

        supplier.Update(code, request.SupplierName, request.SupplierAddress, actorId);

        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}