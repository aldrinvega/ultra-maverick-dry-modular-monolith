using Ultramaverick.Catalog.Application.Commands.Formulas;
using Ultramaverick.Catalog.Application.Import;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;
using Ultramaverick.Catalog.UnitTests.Fakes;
using Ultramaverick.Catalog.UnitTests.Import;

namespace Ultramaverick.Catalog.UnitTests.Application;

public class ImportFormulasCommandHandlerTests
{
    private const string HeaderCode = "FG-1";
    private const string HeaderName = "Header Item";

    private static (ImportFormulasCommandHandler Handler, FakeCatalogUnitOfWork Uow) Build(
        params ImportRow[] rows)
    {
        var uow = new FakeCatalogUnitOfWork();
        uow.ItemsRepo.Seed(Item.Create(HeaderCode, HeaderName, 1, 1, null, false, null));
        uow.ItemsRepo.Seed(Item.Create("RAW-1", "Raw One", 1, 1, null, false, null));
        uow.ItemsRepo.Seed(Item.Create("RAW-2", "Raw Two", 1, 1, null, false, null));

        var handler = new ImportFormulasCommandHandler(
            new FakeImportReader(rows),
            uow,
            new FakeCurrentUser { UserId = 7 });

        return (handler, uow);
    }

    private static ImportRow Row(
        int rowNumber,
        string formulaCode = HeaderCode,
        string formulaName = HeaderName,
        string version = "1",
        string itemCode = "RAW-1",
        string itemDescription = "Raw One",
        string quantity = "10")
        => new(rowNumber, new[] { formulaCode, formulaName, version, itemCode, itemDescription, quantity });

    private static Task<Result<ImportReport>> Import(ImportFormulasCommandHandler handler)
        => handler.Handle(new ImportFormulasCommand(new MemoryStream()), CancellationToken.None);

    [Fact]
    public async Task Healthy_rows_are_grouped_into_one_formula_and_saved_once()
    {
        var (handler, uow) = Build(
            Row(2, itemCode: "RAW-1", itemDescription: "Raw One", quantity: "10"),
            Row(3, itemCode: "RAW-2", itemDescription: "Raw Two", quantity: "5"));

        var result = await Import(handler);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value!.Imported);
        Assert.Equal(0, result.Value!.Skipped);
        Assert.Empty(result.Value!.Failed);
        Assert.Equal(1, uow.SaveCount);

        var formula = Assert.Single(uow.FormulasRepo.All);
        Assert.Equal(1, formula.Version);
        Assert.Equal(15m, formula.Quantity);
        Assert.Equal(2, formula.Requirements.Count);
    }

    [Fact]
    public async Task One_invalid_row_commits_nothing()
    {
        var (handler, uow) = Build(
            Row(2),
            Row(3, itemCode: "RAW-2", itemDescription: "Raw Two", quantity: "0"));

        var result = await Import(handler);

        var report = result.Value!;
        Assert.Equal(0, report.Imported);
        Assert.Equal(0, uow.SaveCount);
        Assert.Empty(uow.FormulasRepo.All);
        Assert.Equal(3, Assert.Single(report.Failed).RowNumber);
    }

    [Fact]
    public async Task Re_uploading_the_same_workbook_skips_the_existing_formula()
    {
        var rows = new[] { Row(2), Row(3, itemCode: "RAW-2", itemDescription: "Raw Two", quantity: "5") };
        var (handler, uow) = Build(rows);

        var first = await Import(handler);
        var second = await Import(handler);

        Assert.Equal(2, first.Value!.Imported);
        Assert.Equal(0, second.Value!.Imported);
        Assert.Equal(2, second.Value!.Skipped);
        Assert.Empty(second.Value!.Failed);
        Assert.Equal(1, uow.SaveCount);
        Assert.Single(uow.FormulasRepo.All);
    }

    [Fact]
    public async Task A_formula_code_that_matches_no_item_is_rejected()
    {
        var (handler, uow) = Build(Row(2, formulaCode: "NOPE"));

        var result = await Import(handler);

        var failure = Assert.Single(result.Value!.Failed);
        Assert.Contains("NOPE", failure.Reason, StringComparison.Ordinal);
        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task A_raw_material_code_that_matches_no_item_is_rejected()
    {
        var (handler, uow) = Build(Row(2, itemCode: "RAW-9", itemDescription: "Nope"));

        var result = await Import(handler);

        var failure = Assert.Single(result.Value!.Failed);
        Assert.Contains("RAW-9", failure.Reason, StringComparison.Ordinal);
        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task A_mismatched_formula_name_is_rejected()
    {
        var (handler, uow) = Build(Row(2, formulaName: "Wrong Name"));

        var result = await Import(handler);

        var failure = Assert.Single(result.Value!.Failed);
        Assert.Contains("Wrong Name", failure.Reason, StringComparison.Ordinal);
        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task A_repeated_requirement_is_rejected_rather_than_imported_twice()
    {
        var (handler, uow) = Build(
            Row(2, itemCode: "RAW-1", itemDescription: "Raw One"),
            Row(3, itemCode: "RAW-1", itemDescription: "Raw One"));

        var result = await Import(handler);

        var failure = Assert.Single(result.Value!.Failed);
        Assert.Equal(3, failure.RowNumber);
        Assert.Equal(0, uow.SaveCount);
    }
}
