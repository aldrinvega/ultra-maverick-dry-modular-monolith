using ClosedXML.Excel;
using Ultramaverick.Catalog.Application.Import;

namespace Ultramaverick.Catalog.UnitTests.Import;

public class XlsxImportReaderTests
{
    private static MemoryStream Workbook(params string[][] rows)
    {
        var stream = new MemoryStream();

        using (var workbook = new XLWorkbook())
        {
            var sheet = workbook.AddWorksheet("Sheet1");

            for (var rowIndex = 0; rowIndex < rows.Length; rowIndex++)
                for (var columnIndex = 0; columnIndex < rows[rowIndex].Length; columnIndex++)
                    sheet.Cell(rowIndex + 1, columnIndex + 1).Value = rows[rowIndex][columnIndex];

            workbook.SaveAs(stream);
        }

        stream.Position = 0;
        return stream;
    }

    [Fact]
    public async Task Rows_after_the_header_are_returned_in_sheet_order()
    {
        using var stream = Workbook(
            new[] { "ItemCode", "ItemDescription" },
            new[] { "RAW-1", "Raw One" },
            new[] { "RAW-2", "Raw Two" });

        var rows = await new XlsxImportReader().ReadAsync(stream, CancellationToken.None);

        Assert.Equal(2, rows.Count);
        Assert.Equal("RAW-1", rows[0].Cell(0));
        Assert.Equal("Raw Two", rows[1].Cell(1));
    }

    [Fact]
    public async Task Blank_rows_are_skipped()
    {
        using var stream = Workbook(
            new[] { "ItemCode", "ItemDescription" },
            new[] { "RAW-1", "Raw One" },
            new[] { "", "" },
            new[] { "RAW-2", "Raw Two" });

        var rows = await new XlsxImportReader().ReadAsync(stream, CancellationToken.None);

        Assert.Equal(2, rows.Count);
    }

    [Fact]
    public async Task A_missing_cell_reads_as_null()
    {
        using var stream = Workbook(
            new[] { "ItemCode", "ItemDescription" },
            new[] { "RAW-1" });

        var rows = await new XlsxImportReader().ReadAsync(stream, CancellationToken.None);

        Assert.Single(rows);
        Assert.Null(rows[0].Cell(1));
    }
}
