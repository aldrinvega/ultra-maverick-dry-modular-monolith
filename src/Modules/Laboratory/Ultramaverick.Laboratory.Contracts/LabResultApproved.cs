using Ultramaverick.SharedKernel;

namespace Ultramaverick.Laboratory.Contracts;

// Distinct from LabResultRecorded: approval is a separate state transition, not a re-recording.
public sealed record LabResultApproved(
    int LabReceiveRequestId,
    string Status,
    int? ApprovedByUserId,
    DateTime OccurredAtUtc) : IDomainEvent;
