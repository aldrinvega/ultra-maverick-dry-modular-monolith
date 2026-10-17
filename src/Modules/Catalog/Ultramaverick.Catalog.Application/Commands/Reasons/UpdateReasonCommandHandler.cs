using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Reasons;

public sealed class UpdateReasonCommandHandler : IRequestHandler<UpdateReasonCommand, Result>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public UpdateReasonCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateReasonCommand request, CancellationToken ct)
    {
        var reason = await _uow.Reasons.GetByIdAsync(request.ReasonId, ct);
        if (reason is null) return Result.Failure("Reason not found.");

        var actorId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required.");

        reason.Update(request.Name, request.MenuId, actorId);

        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}