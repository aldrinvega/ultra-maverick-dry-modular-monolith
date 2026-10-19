using MediatR;
using Microsoft.EntityFrameworkCore;
using Ultramaverick.Manufacturing.Application.Abstractions;
using Ultramaverick.Manufacturing.Application.Models;
using Ultramaverick.Manufacturing.Domain;

namespace Ultramaverick.Manufacturing.Application.Commands.TransformationRequests;

public sealed class PrepareTransformationRequestCommandHandler
    : IRequestHandler<PrepareTransformationRequestCommand, Result>
{
    private readonly IManufacturingUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public PrepareTransformationRequestCommandHandler(
        IManufacturingUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(PrepareTransformationRequestCommand request, CancellationToken ct)
    {
        var transformationRequest = await _uow.TransformationRequests.GetByIdAsync(request.Id, ct);
        if (transformationRequest is null)
            return Result.Failure("Transformation request not found.");

        // The state guard lives here, like CancelOrderCommandHandler: the aggregate transitions
        // unconditionally, the use case refuses a second preparation.
        if (transformationRequest.Status is not (TransformationStatuses.Requested or TransformationStatuses.Approved))
            return Result.Failure("Transformation request is already prepared.");

        var userId = _currentUser.UserId;
        if (userId is null)
            return Result.Failure("An authenticated caller is required.");

        transformationRequest.Prepare(userId.Value);

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure("Transformation request was modified concurrently. Please retry.");
        }

        return Result.Success();
    }
}

