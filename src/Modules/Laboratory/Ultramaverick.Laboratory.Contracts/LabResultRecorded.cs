using Ultramaverick.SharedKernel;

namespace Ultramaverick.Laboratory.Contracts;

// Cross-module integration contract: published by Laboratory, consumed by Reporting (§12.1).
// The WarehouseReceiptId/BatchNo pair lets Reporting project a QC status without a lookup.
public sealed record LabResultRecorded(
    int LabReceiveRequestId,
    int LabTestRequestId,
    int? WarehouseReceiptId,
    string? BatchNo,
    string Result,
    string Status,
    int? AnalyzedByUserId,
    DateTime? DateAnalyzed,
    DateTime OccurredAtUtc) : IDomainEvent;
