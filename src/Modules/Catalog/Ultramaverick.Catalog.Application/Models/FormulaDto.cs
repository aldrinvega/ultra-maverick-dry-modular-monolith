namespace Ultramaverick.Catalog.Application.Models;

public sealed record FormulaRequirementDto(int Id, int RawMaterialId, decimal Quantity);

public sealed record FormulaDto(
    int Id,
    int ItemId,
    int Version,
    decimal Quantity,
    bool IsActive,
    IReadOnlyList<FormulaRequirementDto> Requirements);