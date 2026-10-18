using Ultramaverick.Orders.Application.Commands.Orders;
using Ultramaverick.Orders.Domain.Entities;
using Ultramaverick.Orders.UnitTests.Fakes;

namespace Ultramaverick.Orders.UnitTests.Application;

public class CreateOrderCommandHandlerTests
{
    private static (CreateOrderCommandHandler Handler, FakeOrdersUnitOfWork Uow,
        FakeCustomerLookup Customers) Build(int customerId = 1)
    {
        var uow = new FakeOrdersUnitOfWork();
        var customers = new FakeCustomerLookup();
        customers.ExistingCustomerIds.Add(customerId);

        var handler = new CreateOrderCommandHandler(
            uow, new FakeOrderNumberGenerator(), customers, new FakeCurrentUser { UserId = 7 });

        return (handler, uow, customers);
    }

    private static CreateOrderCommand Command(int customerId = 1, int lineCount = 1)
        => new(
            customerId,
            null,
            null,
            null,
            Enumerable.Range(0, lineCount)
                .Select(i => new OrderLineInput(10 + i, 20, 3m, null))
                .ToList());

    [Fact]
    public async Task Handle_creates_the_order_and_returns_the_new_id()
    {
        var (handler, uow, _) = Build();

        var result = await handler.Handle(Command(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Value);
        Assert.Equal(1, uow.SaveCount);

        var order = Assert.Single(uow.OrdersRepo.All);
        Assert.Equal("SO-20260101-0001", order.OrderNumber);
        Assert.Equal(1, order.CustomerId);
        Assert.Equal(7, order.CreatedByUserId);
        Assert.Single(order.Lines);
        Assert.Single(order.DomainEvents);
    }

    [Fact]
    public async Task Handle_fails_when_the_customer_does_not_exist()
    {
        var (handler, uow, _) = Build(customerId: 1);

        var result = await handler.Handle(Command(customerId: 99), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal(0, uow.SaveCount);
        Assert.Empty(uow.OrdersRepo.All);
    }

    [Fact]
    public async Task Handle_fails_when_there_are_no_lines()
    {
        var (handler, uow, _) = Build();

        var result = await handler.Handle(Command(lineCount: 0), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, uow.SaveCount);
        Assert.Empty(uow.OrdersRepo.All);
    }
}
