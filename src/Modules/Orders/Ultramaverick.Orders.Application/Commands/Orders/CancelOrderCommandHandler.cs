using MediatR;
using Microsoft.EntityFrameworkCore;
using Ultramaverick.Orders.Application.Abstractions;
using Ultramaverick.Orders.Application.Models;
using Ultramaverick.Orders.Domain;

namespace Ultramaverick.Orders.Application.Commands.Orders;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result>
{
    private readonly IOrdersUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public CancelOrderCommandHandler(IOrdersUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken ct)
    {
        var order = await _uow.Orders.GetByIdAsync(request.OrderId, ct);
        if (order is null)
            return Result.Failure("Order not found.");

        if (order.Status == OrderStatuses.Cancelled)
            return Result.Failure("Order is already cancelled.");

        // The cancel audit and status are written through the aggregate; Orders.CancelledOrders
        // is map-only this wave and is never written by this handler.
        order.Cancel(request.Reason, _currentUser.UserId);
        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure("Order was modified concurrently. Please retry.");
        }

        return Result.Success();
    }
}
