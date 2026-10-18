using MediatR;
using Ultramaverick.Laboratory.Application.Models;

namespace Ultramaverick.Laboratory.Application.Commands.LabReceiveRequests;

public sealed record AcceptLabReceiveRequestCommand(
    int LabTestRequestId,
    int? DispositionId,
    DateTime? DateReceived,
    string? Remarks) : IRequest<Result<int>>;
