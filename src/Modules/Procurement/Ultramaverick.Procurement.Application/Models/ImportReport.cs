namespace Ultramaverick.Procurement.Application.Models;

/// <summary>Row-level outcome of a purchase-order workbook import.</summary>
/// <param name="Imported">Number of workbook rows committed.</param>
/// <param name="Skipped">Number of workbook rows skipped because their PO already exists.</param>
/// <param name="Failed">Rows rejected with a reason. When non-empty nothing was committed.</param>
public sealed record ImportReport(int Imported, int Skipped, IReadOnlyList<ImportFailure> Failed);

/// <summary>A single rejected workbook row and why it was rejected.</summary>
public sealed record ImportFailure(int RowNumber, string Reason);
