using MediatR;
using Ultramaverick.Laboratory.Application.Abstractions;
using Ultramaverick.Laboratory.Application.Models;
using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.Application.Commands.LabTestRequests;

public sealed class CreateLabTestRequestCommandHandler
    : IRequestHandler<CreateLabTestRequestCommand, Result<int>>
{
    private readonly ILaboratoryUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public CreateLabTestRequestCommandHandler(ILaboratoryUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateLabTestRequestCommand request, CancellationToken ct)
    {
        var entity = LabTestRequest.Create(
            request.WarehouseReceiptId,
            request.BatchNo,
            request.Quantity,
            request.TestType,
            request.DateRequested,
            request.DateNeeded,
            _currentUser.UserId);

        entity.TagAnalyses(request.AnalysisIds ?? Array.Empty<int>());
        entity.TagParameters(request.ParameterIds ?? Array.Empty<int>());
        entity.TagProductConditions(request.ProductConditionIds ?? Array.Empty<int>());
        entity.TagSampleTypes(request.SampleTypeIds ?? Array.Empty<int>());
        entity.TagSwabTypes(request.SwabTypeIds ?? Array.Empty<int>());

        await _uow.LabTestRequests.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<int>.Success(entity.Id);
    }
}
