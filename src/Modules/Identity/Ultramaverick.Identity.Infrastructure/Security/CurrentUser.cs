using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Ultramaverick.Identity.Application.Abstractions;

namespace Ultramaverick.Identity.Infrastructure.Security
{
    public sealed class CurrentUser : ICurrentUser
    {
        private readonly ClaimsPrincipal? _principal;

        public CurrentUser(IHttpContextAccessor httpContextAccessor) => _principal = httpContextAccessor.HttpContext?.User;

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
