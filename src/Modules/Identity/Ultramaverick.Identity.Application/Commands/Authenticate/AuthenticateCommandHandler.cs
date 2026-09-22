using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Authenticate
{
    public sealed class AuthenticateCommandHandler : IRequestHandler<AuthenticateCommand, Result<AuthenticateResponse>>
    {
        private const string InvalidCredentials = "Username or Password is incorrect!";

        private readonly IUserRepository _users;
        private readonly IModuleRepository _modules;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokens;
        private readonly IRefreshTokenStore _refreshTokens;

        public AuthenticateCommandHandler(
            IUserRepository users,
            IModuleRepository modules,
            IPasswordHasher hasher,
            ITokenService tokens,
            IRefreshTokenStore refreshTokens)
        {
            _users = users;
            _modules = modules;
            _hasher = hasher;
            _tokens = tokens;
            _refreshTokens = refreshTokens;
        }

        public async Task<Result<AuthenticateResponse>> Handle(AuthenticateCommand request, CancellationToken ct)
        {
            var user = await _users.GetByUserNameAsync(request.UserName.Trim(), ct);

            if (user is null || !user.IsActive)
                return Result<AuthenticateResponse>.Failure(InvalidCredentials);

            if (!_hasher.Verify(request.Password, user.Password.Value))
                return Result<AuthenticateResponse>.Failure(InvalidCredentials);

            var moduleNames = await _modules.GetModuleNamesForRoleAsync(user.RoleId, ct);

            var accessToken = _tokens.CreateAccessToken(user, moduleNames);
            var refreshToken = _tokens.CreateRefreshToken();

            await _refreshTokens.IssueAsync(
                user.Id, refreshToken, DateTime.UtcNow.AddDays(_tokens.RefreshTokenDays), ct);

            return Result<AuthenticateResponse>.Success(new AuthenticateResponse(
                user.Id,
                user.FullName,
                user.UserName,
                user.RoleId.ToString(),
                accessToken.Value,
                accessToken.ExpiresAtUtc,
                refreshToken));
        }
    }
}
