using Ultramaverick.Orders.Contracts;
using Ultramaverick.Orders.Domain;
using Ultramaverick.Orders.Domain.Entities;

namespace Ultramaverick.Orders.UnitTests.Domain;

public class OrderTests
{
    private static Order NewOrder(int customerId = 5) => Order.Create(
        "SO-20260101-0001",
        customerId,
        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        null,
        null,
        7,
        new[] { new OrderLineInput(1, 2, 3m, null) });

    [Fact]
    public void Create_sets_draft_and_raises_id_free_OrderCreated()
    {
        var order = NewOrder();

        Assert.Equal(OrderStatuses.Draft, order.Status);
        Assert.True(order.IsActive);
        Assert.Equal(7, order.CreatedByUserId);
        Assert.Single(order.Lines);

        var raised = Assert.IsType<OrderCreated>(Assert.Single(order.DomainEvents));
        Assert.Equal("SO-20260101-0001", raised.OrderNumber);
        Assert.Equal(5, raised.CustomerId);
        Assert.Null(raised.GetType().GetProperty("Id"));
    }

    [Fact]
    public void Create_rejects_a_blank_order_number()
        => Assert.Throws<ArgumentException>(() => Order.Create(
            "  ", 5, null, null, null, 7, new[] { new OrderLineInput(1, 2, 3m, null) }));

    [Fact]
    public void Create_rejects_a_non_positive_customer_id()
        => Assert.Throws<ArgumentOutOfRangeException>(() => NewOrder(customerId: 0));

    [Fact]
    public void Confirm_sets_confirmed_and_raises_OrderConfirmed()
    {
        var order = NewOrder();
        order.ClearDomainEvents();

        order.Confirm();

        Assert.Equal(OrderStatuses.Confirmed, order.Status);

        var raised = Assert.IsType<OrderConfirmed>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.OrderNumber, raised.OrderNumber);
    }

    [Fact]
    public void Cancel_sets_cancelled_audit_and_raises_OrderCancelled()
    {
        var order = NewOrder();
        order.ClearDomainEvents();

        order.Cancel("damaged goods", 9);

        Assert.Equal(OrderStatuses.Cancelled, order.Status);
        Assert.True(order.IsCancelledOrder);
        Assert.Equal(9, order.CancelledByUserId);
        Assert.NotNull(order.CancelledAtUtc);

        var raised = Assert.IsType<OrderCancelled>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.OrderNumber, raised.OrderNumber);
        Assert.Equal("damaged goods", raised.Reason);
    }

    [Fact]
    public void Cancel_twice_leaves_the_order_cancelled()
    {
        var order = NewOrder();
        order.ClearDomainEvents();

        order.Cancel("first", 9);
        order.Cancel("second", 9);

        Assert.Equal(OrderStatuses.Cancelled, order.Status);
        Assert.True(order.IsCancelledOrder);
        Assert.Equal(2, order.DomainEvents.Count);
    }
}
