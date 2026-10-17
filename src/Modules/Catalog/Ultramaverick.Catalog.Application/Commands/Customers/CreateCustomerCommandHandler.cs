using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Commands.Customers;

public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<int>>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IDepartmentDirectory _departments;

    public CreateCustomerCommandHandler(
        ICatalogUnitOfWork uow, ICurrentUser currentUser, IDepartmentDirectory departments)
    {
        _uow = uow;
        _currentUser = currentUser;
        _departments = departments;
    }

    public async Task<Result<int>> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        var code = (request.CustomerCode ?? string.Empty).Trim();
        if (await _uow.Customers.GetByCodeAsync(code, ct) is not null)
            return Result<int>.Failure($"Customer code '{code}' already exists.");

        var reference = await CheckReferencesAsync(request, ct);
        if (reference is not null) return Result<int>.Failure(reference);

        var customer = Customer.Create(
            code, request.CustomerName, request.FarmId, request.CompanyCode, request.CompanyName,
            request.AccountTitleId, request.LocationId, request.DepartmentId,
            request.MobileNumber, request.LeadMan, request.Address, _currentUser.UserId);

        await _uow.Customers.AddAsync(customer, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<int>.Success(customer.Id);
    }

    private async Task<string?> CheckReferencesAsync(CreateCustomerCommand request, CancellationToken ct)
    {
        if (request.FarmId is int farmId && await _uow.Farms.GetByIdAsync(farmId, ct) is null)
            return "Farm does not exist.";
        if (request.AccountTitleId is int accountTitleId
            && await _uow.AccountTitles.GetByIdAsync(accountTitleId, ct) is null)
            return "Account title does not exist.";
        if (request.LocationId is int locationId
            && await _uow.Locations.GetByIdAsync(locationId, ct) is null)
            return "Location does not exist.";
        if (request.DepartmentId is int departmentId
            && !await _departments.ExistsAsync(departmentId, ct))
            return "Department does not exist.";
        return null;
    }
}