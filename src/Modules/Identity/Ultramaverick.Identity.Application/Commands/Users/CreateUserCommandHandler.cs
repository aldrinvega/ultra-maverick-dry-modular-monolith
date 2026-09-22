using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;
using Ultramaverick.Identity.Domain.Entities;
using Ultramaverick.Identity.Domain.ValueObjects;

namespace Ultramaverick.Identity.Application.Commands.Users
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<int>>
    {
        private readonly IIdentityUnitOfWork _uow;
        private readonly IPasswordHasher _hasher;
        private readonly ICurrentUser _currentUser;

        public CreateUserCommandHandler(IIdentityUnitOfWork uow, IPasswordHasher hasher, ICurrentUser currentUser)
        {
            _uow = uow;
            _hasher = hasher;
            _currentUser = currentUser;
        }
        public async Task<Result<int>> Handle(CreateUserCommand request, CancellationToken ct)
        {
            var userName = request.UserName.Trim();

            if (await _uow.Users.GetByUserNameAsync(userName, ct) is not null)
                return Result<int>.Failure($"Username '{userName}' is already taken.");

            if (await _uow.Roles.GetByIdAsync(request.RoleId, ct) is null)
                return Result<int>.Failure("Role does not exist.");

            if (await _uow.Departments.GetByIdAsync(request.DepartmentId, ct) is null)
                return Result<int>.Failure("Department does not exist.");

            var passwordHash = PasswordHash.FromEncoded(_hasher.Hash(request.Password));

            var user = User.Create(
                 request.FullName,
                 userName,
                 passwordHash,
                 request.RoleId,
                 request.DepartmentId,
                 _currentUser.UserId
                );

            await _uow.Users.AddAsync(user, ct);
            await _uow.SaveChangesAsync(ct);

            return Result<int>.Success(user.Id);

        }
    }
}
