namespace Ultramaverick.Orders.Application.Abstractions;

/// <summary>Read-side port used to validate a customer without a project reference to Catalog.</summary>
public interface ICustomerLookup
{
    Task<bool> ExistsAsync(int customerId, CancellationToken ct);
}
