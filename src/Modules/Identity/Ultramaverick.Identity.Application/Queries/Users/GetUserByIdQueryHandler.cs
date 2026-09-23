using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.Users
{
    public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
    {
        private readonly IIdentityUnitOfWork _uow;

        public GetUserByIdQueryHandler(IIdentityUnitOfWork uow) => _uow = uow;

        public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken ct)
        {
            var user = await _uow.Users.GetByIdAsync(request.UserId, ct);
            if (user is null) return Result<UserDto>.Failure("User not found.");

            var role = await _uow.Roles.GetByIdAsync(user.RoleId, ct);
            var department = await _uow.Departments.GetByIdAsync(user.DepartmentId, ct);

            return Result<UserDto>.Success(new UserDto(
                user.Id,
                user.FullName,
                user.UserName,
                user.RoleId,
                role?.Name ?? string.Empty,
                user.DepartmentId,
                department?.Name ?? string.Empty,
                user.IsActive));
        }
    }
}
