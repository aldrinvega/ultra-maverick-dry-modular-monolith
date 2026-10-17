namespace Ultramaverick.Catalog.Application.Models;

public sealed record SupplierDto(int Id, string SupplierCode, string SupplierName, string? SupplierAddress, bool IsActive);