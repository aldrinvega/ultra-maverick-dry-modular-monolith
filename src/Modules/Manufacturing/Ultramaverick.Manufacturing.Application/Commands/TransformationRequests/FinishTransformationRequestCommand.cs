using MediatR;
using Microsoft.EntityFrameworkCore;
using Ultramaverick.Manufacturing.Application.Abstractions;
using Ultramaverick.Manufacturing.Application.Models;

namespace Ultramaverick.Manufacturing.Application.Commands.TransformationRequests;

public sealed record FinishTransformationRequestCommand(int Id) : IRequest<Result>;

public sealed class FinishTransformationRequestCommandHandler
    : IRequestHandler<FinishTransformationRequestCommand, Result>
{
    private readonly IManufacturingUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public FinishTransformationRequestCommandHandler(
        IManufacturingUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(FinishTransformationRequestCommand request, CancellationToken ct)
    {
        var transformationRequest = await _uow.TransformationRequests.GetByIdAsync(request.Id, ct);
        if (transformationRequest is null)
            return Result.Failure("Transformation request not found.");

        try
        {
            transformationRequest.Finish(_currentUser.UserId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

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
