using MediatR;
using Ultramaverick.Laboratory.Application.Models;

namespace Ultramaverick.Laboratory.Application.Commands.LabTestResults;

public sealed record RecordLabResultCommand(
    int LabReceiveRequestId,
    DateTime AnalyzedAt,
    string Result) : IRequest<Result>;
