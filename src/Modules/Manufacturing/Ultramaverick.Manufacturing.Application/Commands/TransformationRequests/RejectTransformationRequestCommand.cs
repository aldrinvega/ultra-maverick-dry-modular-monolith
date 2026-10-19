using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ultramaverick.Manufacturing.Application.Abstractions;
using Ultramaverick.Manufacturing.Application.Models;

namespace Ultramaverick.Manufacturing.Application.Commands.TransformationRequests;

public sealed record RejectTransformationRequestCommand(int Id, int ReasonId, decimal Quantity)
    : IRequest<Result>;

public sealed class RejectTransformationRequestCommandValidator
    : AbstractValidator<RejectTransformationRequestCommand>
{
    public RejectTransformationRequestCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.ReasonId).GreaterThan(0);
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .PrecisionScale(18, 4, ignoreTrailingZeros: false);
    }
}

public sealed class RejectTransformationRequestCommandHandler
    : IRequestHandler<RejectTransformationRequestCommand, Result>
{
    private readonly IManufacturingUnitOfWork _uow;

    public RejectTransformationRequestCommandHandler(IManufacturingUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(RejectTransformationRequestCommand request, CancellationToken ct)
    {
        var transformationRequest = await _uow.TransformationRequests.GetByIdAsync(request.Id, ct);
        if (transformationRequest is null)
            return Result.Failure("Transformation request not found.");

        try
        {
            transformationRequest.Reject(request.ReasonId, request.Quantity);
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
