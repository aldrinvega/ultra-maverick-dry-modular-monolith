namespace Ultramaverick.Procurement.Application.Import;

/// <summary>Transport-level rules for an uploaded workbook, applied before any parsing.</summary>
public static class ImportFileRules
{
    public const long MaxSizeBytes = 10 * 1024 * 1024;

    private const string XlsxContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>Returns the reason the file is rejected, or null when it is acceptable.</summary>
    public static string? Reject(string? fileName, string? contentType, long length)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "A file name is required.";

        if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return "Only .xlsx workbooks are supported.";

        if (!string.IsNullOrWhiteSpace(contentType)
            && !string.Equals(contentType, XlsxContentType, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(contentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase))
        {
            return "The file content type is not an xlsx workbook.";
        }

        if (length <= 0)
            return "The file is empty.";

        if (length > MaxSizeBytes)
            return "The file exceeds the 10 MB limit.";

        return null;
    }
}
