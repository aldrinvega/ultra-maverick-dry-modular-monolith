using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Users
{
    public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result>
    {
        private readonly IIdentityUnitOfWork _uow;
        private readonly ICurrentUser _currentUser;

        public UpdateUserCommandHandler(IIdentityUnitOfWork uow, ICurrentUser currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken);
            if(user is null) return Result.Failure("User not found");

            var userName = request.UserName.Trim();
            var existingUser = await _uow.Users.GetByUserNameAsync(userName, cancellationToken);
            if(existingUser is not null && existingUser.Id != user.Id)
                return Result.Failure($"Username '{userName}' is already taken.");

            if(await _uow.Roles.GetByIdAsync(request.RoleId, cancellationToken) is null)
                return Result.Failure("Role does not exist.");

            if (await _uow.Departments.GetByIdAsync(request.DepartmentId, cancellationToken) is null)
                return Result.Failure("Department does not exist.");

            var actorId = _currentUser.UserId ?? throw new InvalidOperationException("Current user is not authenticated.");

            user.UpdateProfile(
                request.FullName,
                userName,
                request.RoleId,
                request.DepartmentId,
                actorId
            );
            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
