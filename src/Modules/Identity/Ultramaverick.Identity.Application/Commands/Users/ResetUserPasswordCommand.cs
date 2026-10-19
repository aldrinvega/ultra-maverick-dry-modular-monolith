using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;
using Ultramaverick.Identity.Domain.ValueObjects;

namespace Ultramaverick.Identity.Application.Commands.Users
{
    /// <summary>
    /// Sets a new password on an existing account. Used to onboard migrated users, whose
    /// legacy passwords cannot be verified by the current hasher.
    /// </summary>
    public sealed record ResetUserPasswordCommand(string UserName, string NewPassword) : IRequest<Result>;

    public sealed class ResetUserPasswordCommandHandler
        : IRequestHandler<ResetUserPasswordCommand, Result>
    {
        private readonly IIdentityUnitOfWork _uow;
        private readonly IPasswordHasher _hasher;
        private readonly ICurrentUser _currentUser;

        public ResetUserPasswordCommandHandler(
            IIdentityUnitOfWork uow,
            IPasswordHasher hasher,
            ICurrentUser currentUser)
        {
            _uow = uow;
            _hasher = hasher;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserName))
                return Result.Failure("A user name is required.");

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
                return Result.Failure("The new password must be at least 8 characters.");

            var user = await _uow.Users.GetByUserNameAsync(request.UserName.Trim(), cancellationToken);
            if (user is null)
                return Result.Failure("User not found.");

            if (_currentUser.UserId is not { } actorId)
                return Result.Failure("Current user is not authenticated.");

            user.ChangePassword(PasswordHash.FromEncoded(_hasher.Hash(request.NewPassword)), actorId);
            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
