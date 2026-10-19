using Ultramaverick.Catalog.Application.Import;

namespace Ultramaverick.Catalog.UnitTests.Import;

internal sealed class FakeImportReader : IImportReader
{
    private readonly IReadOnlyList<ImportRow> _rows;

    public FakeImportReader(IReadOnlyList<ImportRow> rows) => _rows = rows;

    public Task<IReadOnlyList<ImportRow>> ReadAsync(Stream stream, CancellationToken ct)
        => Task.FromResult(_rows);
}
