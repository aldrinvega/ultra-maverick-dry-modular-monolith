using MediatR;
using Ultramaverick.Manufacturing.Application.Abstractions;
using Ultramaverick.Manufacturing.Application.Models;
using Ultramaverick.Manufacturing.Domain.Entities;

namespace Ultramaverick.Manufacturing.Application.Commands.TransformationRequests;

public sealed class CreateTransformationRequestCommandHandler
    : IRequestHandler<CreateTransformationRequestCommand, Result<int>>
{
    private readonly IManufacturingUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public CreateTransformationRequestCommandHandler(
        IManufacturingUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateTransformationRequestCommand request, CancellationToken ct)
    {
        if (!await _uow.ProductionPlans.ExistsAsync(request.ProductionPlanId, ct))
            return Result<int>.Failure("Production plan not found.");

        if (request.Lines.Count == 0)
            return Result<int>.Failure("A transformation request must have at least one line.");

        var transformationRequest = TransformationRequest.Create(
            request.ProductionPlanId,
            request.WarehouseId,
            _currentUser.UserId,
            request.Lines);

        await _uow.TransformationRequests.AddAsync(transformationRequest, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<int>.Success(transformationRequest.Id);
    }
}
