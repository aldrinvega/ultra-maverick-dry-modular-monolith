using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Customers;

public sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IDepartmentDirectory _departments;

    public UpdateCustomerCommandHandler(
        ICatalogUnitOfWork uow, ICurrentUser currentUser, IDepartmentDirectory departments)
    {
        _uow = uow;
        _currentUser = currentUser;
        _departments = departments;
    }

    public async Task<Result> Handle(UpdateCustomerCommand request, CancellationToken ct)
    {
        var customer = await _uow.Customers.GetByIdAsync(request.CustomerId, ct);
        if (customer is null) return Result.Failure("Customer not found.");

        if (request.FarmId is int farmId && await _uow.Farms.GetByIdAsync(farmId, ct) is null)
            return Result.Failure("Farm does not exist.");
        if (request.AccountTitleId is int accountTitleId
            && await _uow.AccountTitles.GetByIdAsync(accountTitleId, ct) is null)
            return Result.Failure("Account title does not exist.");
        if (request.LocationId is int locationId
            && await _uow.Locations.GetByIdAsync(locationId, ct) is null)
            return Result.Failure("Location does not exist.");
        if (request.DepartmentId is int departmentId
            && !await _departments.ExistsAsync(departmentId, ct))
            return Result.Failure("Department does not exist.");

        var actorId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required.");

        customer.Update(
            request.CustomerName, request.FarmId, request.CompanyCode, request.CompanyName,
            request.AccountTitleId, request.LocationId, request.DepartmentId,
            request.MobileNumber, request.LeadMan, request.Address, actorId);

        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}