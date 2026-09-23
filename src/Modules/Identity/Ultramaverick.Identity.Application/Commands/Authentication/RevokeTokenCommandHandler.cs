using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Authentication
{
    public sealed class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result>
    {
        private readonly IRefreshTokenStore _refreshTokens;

        public RevokeTokenCommandHandler(IRefreshTokenStore refreshTokens)
            => _refreshTokens = refreshTokens;

        public async Task<Result> Handle(RevokeTokenCommand request, CancellationToken ct)
        {
            // Idempotent: revoking an unknown or already-revoked token is not an error.
            await _refreshTokens.RevokeAsync(request.RefreshToken, ct);
            return Result.Success();
        }
    }
}
