using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Customers;

public sealed class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly ICatalogUnitOfWork _uow;

    public GetCustomerByIdQueryHandler(ICatalogUnitOfWork uow) => _uow = uow;

    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken ct)
    {
        var customer = await _uow.Customers.GetByIdAsync(request.CustomerId, ct);
        if (customer is null) return Result<CustomerDto>.Failure("Customer not found.");

        return Result<CustomerDto>.Success(GetCustomersQueryHandler.Map(customer));
    }
}