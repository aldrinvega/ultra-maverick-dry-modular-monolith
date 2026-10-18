using MediatR;
using Ultramaverick.Manufacturing.Application.Models;
using Ultramaverick.Manufacturing.Domain.Entities;

namespace Ultramaverick.Manufacturing.Application.Commands.TransformationRequests;

/// <summary>Creates a transformation request. The requester is taken from the caller, never the body.</summary>
public sealed record CreateTransformationRequestCommand(
    int ProductionPlanId,
    int WarehouseId,
    IReadOnlyList<TransformationLineInput> Lines) : IRequest<Result<int>>;
