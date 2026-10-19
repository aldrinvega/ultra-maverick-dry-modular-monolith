using Ultramaverick.Catalog.Application.Import;

namespace Ultramaverick.Catalog.UnitTests.Import;

public class ImportFileRulesTests
{
    private const string XlsxType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [Fact]
    public void A_well_formed_xlsx_is_accepted()
        => Assert.Null(ImportFileRules.Reject("raw-materials.xlsx", XlsxType, 1024));

    [Fact]
    public void An_octet_stream_content_type_is_accepted_because_browsers_send_it()
        => Assert.Null(ImportFileRules.Reject("raw-materials.XLSX", "application/octet-stream", 1024));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_missing_file_name_is_rejected(string? fileName)
        => Assert.NotNull(ImportFileRules.Reject(fileName, XlsxType, 1024));

    [Theory]
    [InlineData("raw-materials.xls")]
    [InlineData("raw-materials.csv")]
    [InlineData("raw-materials")]
    public void A_non_xlsx_extension_is_rejected(string fileName)
        => Assert.NotNull(ImportFileRules.Reject(fileName, XlsxType, 1024));

    [Fact]
    public void A_wrong_content_type_is_rejected()
        => Assert.NotNull(ImportFileRules.Reject("raw-materials.xlsx", "text/plain", 1024));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void An_empty_file_is_rejected(long length)
        => Assert.NotNull(ImportFileRules.Reject("raw-materials.xlsx", XlsxType, length));

    [Fact]
    public void A_file_over_the_limit_is_rejected()
        => Assert.NotNull(ImportFileRules.Reject(
            "raw-materials.xlsx", XlsxType, ImportFileRules.MaxSizeBytes + 1));

    [Fact]
    public void A_file_exactly_at_the_limit_is_accepted()
        => Assert.Null(ImportFileRules.Reject(
            "raw-materials.xlsx", XlsxType, ImportFileRules.MaxSizeBytes));
}
