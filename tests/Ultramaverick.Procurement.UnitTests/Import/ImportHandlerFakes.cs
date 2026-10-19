using Ultramaverick.Procurement.Application.Abstractions;
using Ultramaverick.Procurement.Application.Import;

namespace Ultramaverick.Procurement.UnitTests.Import;

internal sealed class FakeImportReader : IImportReader
{
    private readonly IReadOnlyList<ImportRow> _rows;

    public FakeImportReader(IReadOnlyList<ImportRow> rows) => _rows = rows;

    public Task<IReadOnlyList<ImportRow>> ReadAsync(Stream stream, CancellationToken ct)
        => Task.FromResult(_rows);
}

internal sealed class FakePurchaseOrderImportLookup : IPurchaseOrderImportLookup
{
    public Dictionary<string, int> SupplierIdsByName { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, ImportedItem> ItemsByCode { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, int> UomIdsByCode { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Task<int?> FindSupplierIdByNameAsync(string supplierName, CancellationToken ct)
        => Task.FromResult(SupplierIdsByName.TryGetValue(supplierName, out var id) ? (int?)id : null);

    public Task<ImportedItem?> FindItemByCodeAsync(string itemCode, CancellationToken ct)
        => Task.FromResult(ItemsByCode.TryGetValue(itemCode, out var item) ? (ImportedItem?)item : null);

    public Task<int?> FindUomIdByCodeAsync(string uomCode, CancellationToken ct)
        => Task.FromResult(UomIdsByCode.TryGetValue(uomCode, out var id) ? (int?)id : null);
}
