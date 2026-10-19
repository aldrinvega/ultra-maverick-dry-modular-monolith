using Ultramaverick.Procurement.Application.Abstractions;
using Ultramaverick.Procurement.Application.Commands.PurchaseOrders;
using Ultramaverick.Procurement.Application.Import;
using Ultramaverick.Procurement.Application.Models;
using Ultramaverick.Procurement.UnitTests.Fakes;
using Ultramaverick.Procurement.UnitTests.Import;

namespace Ultramaverick.Procurement.UnitTests.Application;

public class ImportPurchaseOrdersCommandHandlerTests
{
    private static (ImportPurchaseOrdersCommandHandler Handler, FakeProcurementUnitOfWork Uow) Build(
        params ImportRow[] rows)
    {
        var uow = new FakeProcurementUnitOfWork();

        var lookup = new FakePurchaseOrderImportLookup();
        lookup.SupplierIdsByName["Acme"] = 1;
        lookup.ItemsByCode["ITEM-1"] = new ImportedItem(10, "Item One");
        lookup.ItemsByCode["ITEM-2"] = new ImportedItem(11, "Item Two");
        lookup.UomIdsByCode["KG"] = 20;

        var handler = new ImportPurchaseOrdersCommandHandler(
            new FakeImportReader(rows),
            uow,
            lookup,
            new FakeCurrentUser { UserId = 7 });

        return (handler, uow);
    }

    private static ImportRow Row(
        int rowNumber,
        string poNumber,
        string itemCode = "ITEM-1",
        string itemDescription = "Item One",
        string ordered = "10",
        string unitPrice = "5",
        string vendor = "Acme",
        string uom = "KG",
        string prNumber = "PR-1",
        string poDate = "",
        string prDate = "",
        string delivered = "",
        string billed = "")
        => new(rowNumber, new[]
        {
            prNumber, prDate, poNumber, poDate, itemCode, itemDescription,
            ordered, delivered, billed, uom, unitPrice, vendor
        });

    private static Task<Result<ImportReport>> Import(
        ImportPurchaseOrdersCommandHandler handler)
        => handler.Handle(
            new ImportPurchaseOrdersCommand(new MemoryStream()), CancellationToken.None);

    [Fact]
    public async Task Healthy_rows_are_imported_grouped_by_po_and_saved_once()
    {
        var (handler, uow) = Build(
            Row(2, "PO-1", "ITEM-1"),
            Row(3, "PO-1", "ITEM-2", itemDescription: "Item Two"));

        var result = await Import(handler);

        var report = result.Value!;
        Assert.True(result.Succeeded);
        Assert.Equal(2, report.Imported);
        Assert.Equal(0, report.Skipped);
        Assert.Empty(report.Failed);
        Assert.Equal(1, uow.SaveCount);

        var po = Assert.Single(uow.PurchaseOrdersRepo.All);
        Assert.Equal("PO-1", po.PoNumber);
        Assert.Equal(2, po.Lines.Count);
    }

    [Fact]
    public async Task One_invalid_row_commits_nothing()
    {
        var (handler, uow) = Build(
            Row(2, "PO-1"),
            Row(3, "PO-2", ordered: "0"));

        var result = await Import(handler);

        var report = result.Value!;
        Assert.Equal(0, report.Imported);
        Assert.Equal(0, uow.SaveCount);
        Assert.Empty(uow.PurchaseOrdersRepo.All);
        Assert.Equal(3, Assert.Single(report.Failed).RowNumber);
    }

    [Fact]
    public async Task Re_uploading_the_same_workbook_skips_existing_rows()
    {
        var rows = new[] { Row(2, "PO-1"), Row(3, "PO-2") };
        var (handler, uow) = Build(rows);

        var first = await Import(handler);
        var second = await Import(handler);

        Assert.Equal(2, first.Value!.Imported);
        Assert.Equal(0, second.Value!.Imported);
        Assert.Equal(2, second.Value!.Skipped);
        Assert.Empty(second.Value!.Failed);
        Assert.Equal(1, uow.SaveCount);
        Assert.Equal(2, uow.PurchaseOrdersRepo.All.Count);
    }

    [Fact]
    public async Task An_unknown_supplier_rejects_the_row_without_saving()
    {
        var (handler, uow) = Build(Row(2, "PO-1", vendor: "Nobody"));

        var result = await Import(handler);

        var failure = Assert.Single(result.Value!.Failed);
        Assert.Contains("Nobody", failure.Reason, StringComparison.Ordinal);
        Assert.Equal(0, uow.SaveCount);
        Assert.Empty(uow.PurchaseOrdersRepo.All);
    }

    [Fact]
    public async Task A_non_zero_delivered_quantity_is_rejected_rather_than_dropped()
    {
        var (handler, uow) = Build(Row(2, "PO-1", delivered: "3"));

        var result = await Import(handler);

        var failure = Assert.Single(result.Value!.Failed);
        Assert.Contains("Delivered", failure.Reason, StringComparison.Ordinal);
        Assert.Equal(0, uow.SaveCount);
    }
}
