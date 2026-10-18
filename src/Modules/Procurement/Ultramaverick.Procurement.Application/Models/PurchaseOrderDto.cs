namespace Ultramaverick.Procurement.Application.Models;

public sealed record PurchaseOrderDto(
    int Id,
    string PoNumber,
    string? PrNumber,
    DateTime? PoDate,
    DateTime? PrDate,
    int SupplierId,
    int? ImportBatchId,
    string Status,
    bool IsActive,
    DateTime CreatedAtUtc);
