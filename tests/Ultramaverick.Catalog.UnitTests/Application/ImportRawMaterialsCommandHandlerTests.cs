using Ultramaverick.Catalog.Application.Commands.Items;
using Ultramaverick.Catalog.Application.Import;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;
using Ultramaverick.Catalog.UnitTests.Fakes;
using Ultramaverick.Catalog.UnitTests.Import;

namespace Ultramaverick.Catalog.UnitTests.Application;

public class ImportRawMaterialsCommandHandlerTests
{
    private static (ImportRawMaterialsCommandHandler Handler, FakeCatalogUnitOfWork Uow) Build(
        params ImportRow[] rows)
    {
        var uow = new FakeCatalogUnitOfWork();
        uow.ItemCategoriesRepo.Seed(ItemCategory.Create("Raw Materials", null));
        uow.UomsRepo.Seed(Uom.Create("KG", "Kilogram", null));

        var handler = new ImportRawMaterialsCommandHandler(
            new FakeImportReader(rows),
            uow,
            new FakeCurrentUser { UserId = 7 });

        return (handler, uow);
    }

    private static ImportRow Row(
        int rowNumber,
        string itemCode,
        string itemDescription = "Raw Material",
        string category = "Raw Materials",
        string uom = "KG",
        string buffer = "5")
        => new(rowNumber, new[] { itemCode, itemDescription, category, uom, buffer });

    private static Task<Result<ImportReport>> Import(ImportRawMaterialsCommandHandler handler)
        => handler.Handle(
            new ImportRawMaterialsCommand(new MemoryStream()), CancellationToken.None);

    [Fact]
    public async Task Healthy_rows_are_imported_and_saved_once()
    {
        var (handler, uow) = Build(
            Row(2, "RAW-1"),
            Row(3, "RAW-2", "Raw Material Two"));

        var result = await Import(handler);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value!.Imported);
        Assert.Equal(0, result.Value!.Skipped);
        Assert.Empty(result.Value!.Failed);
        Assert.Equal(1, uow.SaveCount);
        Assert.Equal(2, uow.ItemsRepo.All.Count);
    }

    [Fact]
    public async Task One_invalid_row_commits_nothing()
    {
        var (handler, uow) = Build(
            Row(2, "RAW-1"),
            Row(3, "RAW-2", buffer: "not-a-number"));

        var result = await Import(handler);

        var report = result.Value!;
        Assert.Equal(0, report.Imported);
        Assert.Equal(0, uow.SaveCount);
        Assert.Empty(uow.ItemsRepo.All);
        Assert.Equal(3, Assert.Single(report.Failed).RowNumber);
    }

    [Fact]
    public async Task Re_uploading_the_same_workbook_skips_existing_rows()
    {
        var rows = new[] { Row(2, "RAW-1"), Row(3, "RAW-2", "Raw Material Two") };
        var (handler, uow) = Build(rows);

        var first = await Import(handler);
        var second = await Import(handler);

        Assert.Equal(2, first.Value!.Imported);
        Assert.Equal(0, second.Value!.Imported);
        Assert.Equal(2, second.Value!.Skipped);
        Assert.Empty(second.Value!.Failed);
        Assert.Equal(1, uow.SaveCount);
        Assert.Equal(2, uow.ItemsRepo.All.Count);
    }

    [Fact]
    public async Task An_unknown_item_category_rejects_the_row_without_saving()
    {
        var (handler, uow) = Build(Row(2, "RAW-1", category: "Unknown"));

        var result = await Import(handler);

        var failure = Assert.Single(result.Value!.Failed);
        Assert.Contains("Unknown", failure.Reason, StringComparison.Ordinal);
        Assert.Equal(0, uow.SaveCount);
        Assert.Empty(uow.ItemsRepo.All);
    }

    [Fact]
    public async Task An_unknown_uom_rejects_the_row_without_saving()
    {
        var (handler, uow) = Build(Row(2, "RAW-1", uom: "XX"));

        var result = await Import(handler);

        var failure = Assert.Single(result.Value!.Failed);
        Assert.Contains("XX", failure.Reason, StringComparison.Ordinal);
        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task A_uom_may_be_resolved_by_description()
    {
        var (handler, _) = Build(Row(2, "RAW-1", uom: "Kilogram"));

        var result = await Import(handler);

        Assert.Empty(result.Value!.Failed);
        Assert.Equal(1, result.Value!.Imported);
    }

    [Fact]
    public async Task A_duplicated_item_code_is_rejected_rather_than_imported_twice()
    {
        var (handler, uow) = Build(
            Row(2, "RAW-1"),
            Row(3, "RAW-1"));

        var result = await Import(handler);

        var failure = Assert.Single(result.Value!.Failed);
        Assert.Equal(3, failure.RowNumber);
        Assert.Equal(0, uow.SaveCount);
    }
}
