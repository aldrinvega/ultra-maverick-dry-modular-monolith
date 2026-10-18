using MediatR;
using Ultramaverick.Laboratory.Application.Abstractions;
using Ultramaverick.Laboratory.Application.Models;
using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.Application.Commands.LabTestResults;

public sealed class RecordLabResultCommandHandler
    : IRequestHandler<RecordLabResultCommand, Result>
{
    private readonly ILaboratoryUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public RecordLabResultCommandHandler(ILaboratoryUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(RecordLabResultCommand request, CancellationToken ct)
    {
        var receiveRequest = await _uow.LabReceiveRequests
            .GetByIdAsync(request.LabReceiveRequestId, ct);
        if (receiveRequest is null)
            return Result.Failure(
                $"Lab receive request '{request.LabReceiveRequestId}' does not exist.");

        var testRequest = await _uow.LabTestRequests
            .GetByIdAsync(receiveRequest.LabTestRequestId, ct);
        if (testRequest is null)
            return Result.Failure(
                $"Lab test request '{receiveRequest.LabTestRequestId}' does not exist.");

        var result = LabTestResult.Record(
            receiveRequest.Id,
            testRequest.Id,
            testRequest.WarehouseReceiptId,
            testRequest.BatchNo,
            request.AnalyzedAt,
            request.Result,
            _currentUser.UserId);

        await _uow.LabTestResults.AddAsync(result, ct);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
