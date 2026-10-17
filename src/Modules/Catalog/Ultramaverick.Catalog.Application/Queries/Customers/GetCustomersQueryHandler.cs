using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Queries.Customers;

public sealed class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PagedResult<CustomerDto>>
{
    private readonly ICatalogUnitOfWork _uow;

    public GetCustomersQueryHandler(ICatalogUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;
        var skip = (page - 1) * pageSize;

        var rows = await _uow.Customers.ListAsync(request.Search, request.IsActive, skip, pageSize, ct);
        var total = await _uow.Customers.CountAsync(request.Search, request.IsActive, ct);

        var items = rows.Select(Map).ToList();

        return new PagedResult<CustomerDto>(items, page, pageSize, total);
    }

    internal static CustomerDto Map(Customer customer) => new(
        customer.Id, customer.CustomerCode, customer.CustomerName, customer.FarmId,
        customer.CompanyCode, customer.CompanyName, customer.AccountTitleId, customer.LocationId,
        customer.DepartmentId, customer.MobileNumber, customer.LeadMan, customer.Address,
        customer.IsActive);
}