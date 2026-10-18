namespace Ultramaverick.Orders.Application.Abstractions;

public interface IOrderNumberGenerator
{
    Task<string> NextAsync(DateTime utcNow, CancellationToken ct);
}
