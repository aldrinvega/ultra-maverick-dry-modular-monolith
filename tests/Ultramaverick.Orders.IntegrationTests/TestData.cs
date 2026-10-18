using Microsoft.EntityFrameworkCore;
using Ultramaverick.Orders.Domain.Entities;
using Ultramaverick.Orders.Persistence;

namespace Ultramaverick.Orders.IntegrationTests;

internal static class TestData
{
    /// <summary>Creates an order and returns its id, so the outbox selector has a target.</summary>
    public static async Task<int> CreateOrderAsync(OrdersDbContext context)
        => await CreateOrderAsync(context, OrderNumber("SO"));

    /// <summary>Creates an order under an explicit number (used by the generator tests).</summary>
    public static async Task<int> CreateOrderAsync(OrdersDbContext context, string orderNumber)
    {
        var order = Order.Create(
            orderNumber,
            customerId: 1,
            orderDate: DateTime.UtcNow,
            deliveryDate: null,
            remarks: null,
            createdByUserId: 7,
            lines: new[] { new OrderLineInput(10, 20, 5m, null) });

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        return order.Id;
    }

    public static async Task ClearOutboxAsync(OrdersDbContext context)
    {
        context.OutboxMessages.RemoveRange(context.OutboxMessages);
        await context.SaveChangesAsync();
    }

    /// <summary>Removes every order (lines first, to satisfy the FK) without touching the schema.</summary>
    public static async Task ClearOrdersAsync(OrdersDbContext context)
    {
        await context.OrderLines.ExecuteDeleteAsync();
        await context.Orders.ExecuteDeleteAsync();
    }

    public static string OrderNumber(string prefix) => $"{prefix}{Guid.NewGuid():N}"[..20];
}
