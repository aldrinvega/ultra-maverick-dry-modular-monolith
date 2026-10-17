using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id, CancellationToken ct);
    Task<Customer?> GetByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<Customer>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct);
    Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct);
    Task AddAsync(Customer customer, CancellationToken ct);
}