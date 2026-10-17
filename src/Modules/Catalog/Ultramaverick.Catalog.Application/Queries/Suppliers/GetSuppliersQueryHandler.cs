using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Suppliers;

public sealed class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, PagedResult<SupplierDto>>
{
    private readonly ICatalogUnitOfWork _uow;

    public GetSuppliersQueryHandler(ICatalogUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<SupplierDto>> Handle(GetSuppliersQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;
        var skip = (page - 1) * pageSize;

        var rows = await _uow.Suppliers.ListAsync(request.Search, request.IsActive, skip, pageSize, ct);
        var total = await _uow.Suppliers.CountAsync(request.Search, request.IsActive, ct);

        var items = rows
            .Select(s => new SupplierDto(s.Id, s.SupplierCode, s.SupplierName, s.SupplierAddress, s.IsActive))
            .ToList();

        return new PagedResult<SupplierDto>(items, page, pageSize, total);
    }
}