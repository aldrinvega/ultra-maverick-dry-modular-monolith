using ClosedXML.Excel;

namespace Ultramaverick.Procurement.Application.Import;

public interface IImportReader
{
    Task<IReadOnlyList<ImportRow>> ReadAsync(Stream stream, CancellationToken ct);
}

public sealed record ImportRow(int RowNumber, IReadOnlyList<string?> Cells)
{
    public string? Cell(int index) => index >= 0 && index < Cells.Count ? Cells[index] : null;
}

/// <summary>Reads the first worksheet of an xlsx stream, treating row one as headers.</summary>
public sealed class XlsxImportReader : IImportReader
{
    public Task<IReadOnlyList<ImportRow>> ReadAsync(Stream stream, CancellationToken ct)
    {
        var rows = new List<ImportRow>();

        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheets.FirstOrDefault();
        var used = sheet?.RangeUsed();

        if (used is null)
            return Task.FromResult<IReadOnlyList<ImportRow>>(rows);

        var columnCount = used.ColumnCount();

        foreach (var row in used.Rows())
        {
            ct.ThrowIfCancellationRequested();

            if (row.RowNumber() == used.FirstRow().RowNumber())
                continue;

            var cells = row.Cells(1, columnCount)
                .Select(cell => { var value = cell.GetFormattedString().Trim(); return string.IsNullOrEmpty(value) ? null : value; })
                .ToList();

            if (cells.All(string.IsNullOrEmpty))
                continue;

            rows.Add(new ImportRow(row.RowNumber(), cells));
        }

        return Task.FromResult<IReadOnlyList<ImportRow>>(rows);
    }
}

