using MediatR;
using Ultramaverick.Orders.Application.Abstractions;
using Ultramaverick.Orders.Application.Models;
using Ultramaverick.Orders.Domain.Entities;

namespace Ultramaverick.Orders.Application.Commands.Orders;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<int>>
{
    private readonly IOrdersUnitOfWork _uow;
    private readonly IOrderNumberGenerator _numbers;
    private readonly ICustomerLookup _customers;
    private readonly ICurrentUser _currentUser;

    public CreateOrderCommandHandler(
        IOrdersUnitOfWork uow,
        IOrderNumberGenerator numbers,
        ICustomerLookup customers,
        ICurrentUser currentUser)
    {
        _uow = uow;
        _numbers = numbers;
        _customers = customers;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        if (!await _customers.ExistsAsync(request.CustomerId, ct))
            return Result<int>.Failure("Customer not found.");

        if (request.Lines.Count == 0)
            return Result<int>.Failure("An order must have at least one line.");

        var orderNumber = await _numbers.NextAsync(DateTime.UtcNow, ct);

        var order = Order.Create(
            orderNumber,
            request.CustomerId,
            request.OrderDate,
            request.DeliveryDate,
            request.Remarks,
            _currentUser.UserId,
            request.Lines);

        await _uow.Orders.AddAsync(order, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<int>.Success(order.Id);
    }
}
