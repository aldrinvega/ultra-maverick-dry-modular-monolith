using MediatR;
using Ultramaverick.Laboratory.Application.Models;

namespace Ultramaverick.Laboratory.Application.Commands.LabTestRequests;

public sealed record CreateLabTestRequestCommand(
    int? WarehouseReceiptId,
    string? BatchNo,
    decimal Quantity,
    string? TestType,
    DateTime? DateRequested,
    DateTime? DateNeeded,
    int[]? AnalysisIds,
    int[]? ParameterIds,
    int[]? ProductConditionIds,
    int[]? SampleTypeIds,
    int[]? SwabTypeIds) : IRequest<Result<int>>;
