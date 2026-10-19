using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;
using Ultramaverick.Identity.Domain.ValueObjects;

namespace Ultramaverick.Identity.Application.Commands.Authentication
{
    public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword)
        : IRequest<Result<AuthenticateResponse>>;

    public sealed class ChangePasswordCommandHandler
        : IRequestHandler<ChangePasswordCommand, Result<AuthenticateResponse>>
    {
        private readonly IIdentityUnitOfWork _uow;
        private readonly IModuleRepository _modules;
        private readonly IPasswordHasher _hasher;
        private readonly ICurrentUser _currentUser;
        private readonly ITokenService _tokens;
        private readonly IRefreshTokenStore _refreshTokens;

        public ChangePasswordCommandHandler(
            IIdentityUnitOfWork uow,
            IModuleRepository modules,
            IPasswordHasher hasher,
            ICurrentUser currentUser,
            ITokenService tokens,
            IRefreshTokenStore refreshTokens)
        {
            _uow = uow;
            _modules = modules;
            _hasher = hasher;
            _currentUser = currentUser;
            _tokens = tokens;
            _refreshTokens = refreshTokens;
        }

        public async Task<Result<AuthenticateResponse>> Handle(ChangePasswordCommand request, CancellationToken ct)
        {
            if (_currentUser.UserId is not { } userId)
                return Result<AuthenticateResponse>.Failure("Current user is not authenticated.");

            var user = await _uow.Users.GetByIdAsync(userId, ct);
            if (user is null || !user.IsActive)
                return Result<AuthenticateResponse>.Failure("User not found.");

            if (!_hasher.Verify(request.CurrentPassword, user.Password.Value))
                return Result<AuthenticateResponse>.Failure("Current password is incorrect.");

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
                return Result<AuthenticateResponse>.Failure("The new password must be at least 8 characters.");

            if (request.NewPassword == request.CurrentPassword)
                return Result<AuthenticateResponse>.Failure("The new password must be different from the current password.");

            user.ChangePassword(PasswordHash.FromEncoded(_hasher.Hash(request.NewPassword)), userId);
            await _uow.SaveChangesAsync(ct);

            var role = await _uow.Roles.GetByIdAsync(user.RoleId, ct);
            var moduleNames = await _modules.GetModuleNamesForRoleAsync(user.RoleId, ct);

            var accessToken = _tokens.CreateAccessToken(user, moduleNames);
            var refreshToken = _tokens.CreateRefreshToken();

            await _refreshTokens.IssueAsync(
                user.Id, refreshToken, DateTime.UtcNow.AddDays(_tokens.RefreshTokenDays), ct);

            return Result<AuthenticateResponse>.Success(new AuthenticateResponse(
                user.Id,
                user.FullName,
                user.UserName,
                user.RoleId,
                role?.Name ?? string.Empty,
                accessToken.Value,
                accessToken.ExpiresAtUtc,
                refreshToken,
                user.MustChangePassword));
        }
    }
}
