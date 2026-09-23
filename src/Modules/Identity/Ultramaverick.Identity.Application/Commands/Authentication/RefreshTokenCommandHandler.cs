using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Authentication
{
    public sealed class RefreshTokenCommandHandler
        : IRequestHandler<RefreshTokenCommand, Result<AuthenticateResponse>>
    {
        private const string InvalidToken = "Refresh token is invalid, expired, or already used.";

        private readonly IUserRepository _users;
        private readonly IRoleRepository _roles;
        private readonly IModuleRepository _modules;
        private readonly ITokenService _tokens;
        private readonly IRefreshTokenStore _refreshTokens;

        public RefreshTokenCommandHandler(
            IUserRepository users,
            IRoleRepository roles,
            IModuleRepository modules,
            ITokenService tokens,
            IRefreshTokenStore refreshTokens)
        {
            _users = users;
            _roles = roles;
            _modules = modules;
            _tokens = tokens;
            _refreshTokens = refreshTokens;
        }

        public async Task<Result<AuthenticateResponse>> Handle(
            RefreshTokenCommand request, CancellationToken ct)
        {
            var newRefreshToken = _tokens.CreateRefreshToken();
            var newExpiresAtUtc = DateTime.UtcNow.AddDays(_tokens.RefreshTokenDays);

            // Atomic rotation: the presented token is consumed and its replacement issued
            // in a single transaction, so a failure cannot leave the user without a token.
            var userId = await _refreshTokens.RotateAsync(
                request.RefreshToken, newRefreshToken, newExpiresAtUtc, ct);

            if (userId is null)
                return Result<AuthenticateResponse>.Failure(InvalidToken);

            var user = await _users.GetByIdAsync(userId.Value, ct);
            if (user is null || !user.IsActive)
                return Result<AuthenticateResponse>.Failure(InvalidToken);

            var role = await _roles.GetByIdAsync(user.RoleId, ct);
            var moduleNames = await _modules.GetModuleNamesForRoleAsync(user.RoleId, ct);

            var accessToken = _tokens.CreateAccessToken(user, moduleNames);

            return Result<AuthenticateResponse>.Success(new AuthenticateResponse(
                user.Id,
                user.FullName,
                user.UserName,
                user.RoleId,
                role?.Name ?? string.Empty,
                accessToken.Value,
                accessToken.ExpiresAtUtc,
                newRefreshToken));
        }
    }
}
