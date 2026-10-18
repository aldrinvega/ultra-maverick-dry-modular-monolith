using MediatR;
using Ultramaverick.Laboratory.Application.Abstractions;
using Ultramaverick.Laboratory.Application.Models;
using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.Application.Commands.LabReceiveRequests;

public sealed class AcceptLabReceiveRequestCommandHandler
    : IRequestHandler<AcceptLabReceiveRequestCommand, Result<int>>
{
    private readonly ILaboratoryUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public AcceptLabReceiveRequestCommandHandler(
        ILaboratoryUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(
        AcceptLabReceiveRequestCommand request, CancellationToken ct)
    {
        var testRequest = await _uow.LabTestRequests.GetByIdAsync(request.LabTestRequestId, ct);
        if (testRequest is null)
            return Result<int>.Failure(
                $"Lab test request '{request.LabTestRequestId}' does not exist.");

        var entity = LabReceiveRequest.Create(
            testRequest.Id,
            request.DispositionId,
            request.DateReceived,
            _currentUser.UserId,
            request.Remarks);

        await _uow.LabReceiveRequests.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<int>.Success(entity.Id);
    }
}
