using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Ultramaverick.Catalog.Application.Abstractions;

namespace Ultramaverick.Catalog.Infrastructure.Security
{
    /// <summary>
    /// Catalog's view of the authenticated caller, read from the request principal.
    /// Each module owns its port so the modules stay independent.
    /// </summary>
    public sealed class CatalogCurrentUser : ICurrentUser
    {
        private readonly ClaimsPrincipal? _principal;

        public CatalogCurrentUser(IHttpContextAccessor httpContextAccessor)
            => _principal = httpContextAccessor.HttpContext?.User;

        public int? UserId
        {
            get
            {
                var value = _principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? _principal?.FindFirst("sub")?.Value;

                return int.TryParse(value, out var id) ? id : null;
            }
        }

        public string? UserName => _principal?.FindFirst(ClaimTypes.Name)?.Value;

        public bool IsAuthenticated => _principal?.Identity?.IsAuthenticated == true;
    }
}