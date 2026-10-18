using Ultramaverick.Orders.Application.Commands.Orders;
using Ultramaverick.Orders.Domain;
using Ultramaverick.Orders.Domain.Entities;
using Ultramaverick.Orders.UnitTests.Fakes;

namespace Ultramaverick.Orders.UnitTests.Application;

public class CancelOrderCommandHandlerTests
{
    private static Order NewOrder() => Order.Create(
        "SO-20260101-0001", 5, null, null, null, 7,
        new[] { new OrderLineInput(1, 2, 3m, null) });

    [Fact]
    public async Task Handle_fails_when_the_order_is_not_found()
    {
        var uow = new FakeOrdersUnitOfWork();
        var handler = new CancelOrderCommandHandler(uow, new FakeCurrentUser());

        var result = await handler.Handle(new CancelOrderCommand(5, "changed"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task Handle_fails_when_the_order_is_already_cancelled()
    {
        var uow = new FakeOrdersUnitOfWork();
        var order = NewOrder();
        order.Cancel("first", 9);       // cancel-twice guard: the second cancel must be rejected
        order.ClearDomainEvents();
        uow.OrdersRepo.Seed(5, order);

        var handler = new CancelOrderCommandHandler(uow, new FakeCurrentUser());

        var result = await handler.Handle(new CancelOrderCommand(5, "second"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task Handle_cancels_the_order_and_saves_once()
    {
        var uow = new FakeOrdersUnitOfWork();
        uow.OrdersRepo.Seed(5, NewOrder());

        var handler = new CancelOrderCommandHandler(uow, new FakeCurrentUser { UserId = 9 });

        var result = await handler.Handle(new CancelOrderCommand(5, "changed"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, uow.SaveCount);

        var stored = await uow.OrdersRepo.GetByIdAsync(5, CancellationToken.None);
        Assert.NotNull(stored);
        Assert.Equal(OrderStatuses.Cancelled, stored!.Status);
        Assert.True(stored.IsCancelledOrder);
        Assert.Equal(9, stored.CancelledByUserId);
    }
}
