using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Identity.Application.Abstractions;

namespace Ultramaverick.Api.Integration
{
    /// <summary>
    /// Bridges Catalog's cross-context department port to Identity's query surface at the
    /// composition root, so neither module depends on the other.
    /// </summary>
    public sealed class CatalogDepartmentDirectory : IDepartmentDirectory
    {
        private readonly IIdentityUnitOfWork _identity;

        public CatalogDepartmentDirectory(IIdentityUnitOfWork identity) => _identity = identity;

        public async Task<bool> ExistsAsync(int departmentId, CancellationToken ct)
            => await _identity.Departments.GetByIdAsync(departmentId, ct) is not null;
    }
}