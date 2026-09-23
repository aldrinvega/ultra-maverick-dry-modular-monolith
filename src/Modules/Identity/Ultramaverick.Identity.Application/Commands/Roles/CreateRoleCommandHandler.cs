using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;
using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Application.Commands.Roles
{
    public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<int>>
    {
        private readonly IIdentityUnitOfWork _uow;
        private readonly ICurrentUser _currentUser;

        public CreateRoleCommandHandler(IIdentityUnitOfWork uow, ICurrentUser currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result<int>> Handle(CreateRoleCommand request, CancellationToken ct)
        {
            var name = request.Name.Trim();

            if (await _uow.Roles.GetByNameAsync(name, ct) is not null)
                return Result<int>.Failure($"Role '{name}' already exists.");

            var actorId = _currentUser.UserId
                ?? throw new InvalidOperationException("An authenticated user is required.");

            var role = Role.Create(name, actorId);

            await _uow.Roles.AddAsync(role, ct);
            await _uow.SaveChangesAsync(ct);

            return Result<int>.Success(role.Id);
        }
    }
}
