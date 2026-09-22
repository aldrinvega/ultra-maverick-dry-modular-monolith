using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Users
{
    public sealed class SetUserActiveCommandHandler : IRequestHandler<SetUserActiveCommand, Result>
    {
        private readonly IIdentityUnitOfWork _ouw;
        private readonly ICurrentUser _currentUser;

        public SetUserActiveCommandHandler(IIdentityUnitOfWork ouw, ICurrentUser currentUser)
        {
            _ouw = ouw;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(SetUserActiveCommand request, CancellationToken ct)
        {
            var user = await _ouw.Users.GetByIdAsync(request.UserId, ct);
            if (user is null) return Result.Failure("User not found");

            var actorId = _currentUser.UserId ?? throw new InvalidOperationException("Current user is not authenticated.");

            if (request.IsActive) user.Activate(actorId);
            else user.Deactivate(actorId);

            await _ouw.SaveChangesAsync(ct);
            return Result.Success();
        }
    }
}
