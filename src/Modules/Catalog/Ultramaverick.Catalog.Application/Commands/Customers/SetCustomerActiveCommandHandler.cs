using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Customers;

public sealed class SetCustomerActiveCommandHandler : IRequestHandler<SetCustomerActiveCommand, Result>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public SetCustomerActiveCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(SetCustomerActiveCommand request, CancellationToken ct)
    {
        var customer = await _uow.Customers.GetByIdAsync(request.CustomerId, ct);
        if (customer is null) return Result.Failure("Customer not found.");

        var actorId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required.");

        customer.SetActive(request.IsActive, actorId);

        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}