using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Suppliers;

public sealed class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, Result<SupplierDto>>
{
    private readonly ICatalogUnitOfWork _uow;

    public GetSupplierByIdQueryHandler(ICatalogUnitOfWork uow) => _uow = uow;

    public async Task<Result<SupplierDto>> Handle(GetSupplierByIdQuery request, CancellationToken ct)
    {
        var supplier = await _uow.Suppliers.GetByIdAsync(request.SupplierId, ct);
        if (supplier is null) return Result<SupplierDto>.Failure("Supplier not found.");

        return Result<SupplierDto>.Success(
            new SupplierDto(supplier.Id, supplier.SupplierCode, supplier.SupplierName, supplier.SupplierAddress, supplier.IsActive));
    }
}