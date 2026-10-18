using Ultramaverick.Quality.Domain.Entities;

namespace Ultramaverick.Quality.Application.Models;

public sealed record QcReceiptDto(
    int Id,
    int PurchaseOrderId,
    int? PurchaseOrderLineId,
    int ItemId,
    int UomId,
    int? ProductTypeId,
    string? BatchNo,
    DateTime? ManufacturingDate,
    DateTime? ExpectedDeliveryDate,
    DateTime? ExpiryDate,
    decimal ActualDelivered,
    decimal TotalReject,
    bool IsNearlyExpire,
    string Status,
    bool IsActive)
{
    public static QcReceiptDto From(QcReceipt r) => new(
        r.Id,
        r.PurchaseOrderId,
        r.PurchaseOrderLineId,
        r.ItemId,
        r.UomId,
        r.ProductTypeId,
        r.BatchNo,
        r.ManufacturingDate,
        r.ExpectedDeliveryDate,
        r.ExpiryDate,
        r.ActualDelivered,
        r.TotalReject,
        r.IsNearlyExpire,
        r.Status,
        r.IsActive);
}
