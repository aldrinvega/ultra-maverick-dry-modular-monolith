namespace Ultramaverick.Orders.Application.Abstractions;

/// <summary>Aggregates every Orders repository behind one transaction boundary.</summary>
public interface IOrdersUnitOfWork
{
    IOrderRepository Orders { get; }

    Task<int> SaveChangesAsync(CancellationToken ct);
}
