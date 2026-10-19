using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.IntegrationTests;

/// <summary>
/// Boots the real API in-process against the throwaway database the fixture migrates, then posts
/// generated legacy workbooks through the Catalog upload endpoints.
/// </summary>
public sealed class CatalogImportApiTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private const string Issuer = "https://test-issuer";
    private const string Audience = "test-audience";
    private const string Key = "test-signing-key-for-authorization-tests-0123456789";
    private const string XlsxContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private const string RawMaterialsRoute = "/api/Catalog/UploadRawMaterialsReport";
    private const string FormulaRoute = "/api/Catalog/UploadFormulaReport";

    private static readonly string CategoryName = $"Cat{Guid.NewGuid():N}"[..16];
    private static readonly string HeaderCode = $"FG{Guid.NewGuid():N}"[..10];
    private static readonly string RawOneCode = $"R1{Guid.NewGuid():N}"[..10];
    private static readonly string RawTwoCode = $"R2{Guid.NewGuid():N}"[..10];

    private static readonly string[] RawMaterialsHeader =
    {
        "Item Code", "Item Description", "Item Category", "UOM", "Buffer Level"
    };

    private static readonly string[] FormulaHeader =
    {
        "Formula Code", "Formula Name", "Version", "Item Code", "Item Description", "Quantity"
    };

    private static readonly SemaphoreSlim SeedGate = new(1, 1);
    private static bool _seeded;

    private readonly DatabaseFixture _fixture;
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public CatalogImportApiTests(DatabaseFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        await EnsureSeededAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Default", _fixture.ConnectionString);
            builder.UseSetting("Jwt:Issuer", Issuer);
            builder.UseSetting("Jwt:Audience", Audience);
            builder.UseSetting("Jwt:Key", Key);
        });

        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateToken());
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private async Task EnsureSeededAsync()
    {
        if (_seeded)
            return;

        await SeedGate.WaitAsync();
        try
        {
            if (_seeded)
                return;

            await using var context = _fixture.CreateContext();

            var category = ItemCategory.Create(CategoryName, null);
            var uom = Uom.Create("KG", "Kilogram", null);
            context.ItemCategories.Add(category);
            context.Uoms.Add(uom);
            await context.SaveChangesAsync();

            context.Items.AddRange(
                Item.Create(HeaderCode, "Header Item", category.Id, uom.Id, null, false, null),
                Item.Create(RawOneCode, "Raw One", category.Id, uom.Id, null, false, null),
                Item.Create(RawTwoCode, "Raw Two", category.Id, uom.Id, null, false, null));
            await context.SaveChangesAsync();

            _seeded = true;
        }
        finally
        {
            SeedGate.Release();
        }
    }

    [Fact]
    public async Task A_healthy_raw_materials_workbook_is_imported_and_a_re_upload_is_skipped()
    {
        var code = $"NR{Guid.NewGuid():N}"[..10];
        var workbook = Workbook(RawMaterialsHeader, RawMaterialRow(code));

        var first = await PostAsync(RawMaterialsRoute, workbook);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var firstReport = await first.Content.ReadFromJsonAsync<ImportReport>();
        Assert.NotNull(firstReport);
        Assert.Equal(1, firstReport!.Imported);
        Assert.Equal(0, firstReport.Skipped);
        Assert.Empty(firstReport.Failed);

        await using (var context = _fixture.CreateContext())
            Assert.NotNull(await context.Items.AsNoTracking()
                .FirstOrDefaultAsync(i => i.ItemCode == code));

        var second = await PostAsync(RawMaterialsRoute, workbook);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var secondReport = await second.Content.ReadFromJsonAsync<ImportReport>();
        Assert.NotNull(secondReport);
        Assert.Equal(0, secondReport!.Imported);
        Assert.Equal(1, secondReport.Skipped);
        Assert.Empty(secondReport.Failed);
    }

    [Fact]
    public async Task A_raw_materials_workbook_with_one_invalid_row_returns_422_and_commits_nothing()
    {
        var good = $"NR{Guid.NewGuid():N}"[..10];
        var bad = $"NR{Guid.NewGuid():N}"[..10];
        var workbook = Workbook(
            RawMaterialsHeader,
            RawMaterialRow(good),
            RawMaterialRow(bad, buffer: "not-a-number"));

        var response = await PostAsync(RawMaterialsRoute, workbook);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var report = await response.Content.ReadFromJsonAsync<ImportReport>();
        Assert.NotNull(report);
        Assert.Equal(0, report!.Imported);
        Assert.Single(report.Failed);

        await using var context = _fixture.CreateContext();
        Assert.Null(await context.Items.AsNoTracking().FirstOrDefaultAsync(i => i.ItemCode == good));
        Assert.Null(await context.Items.AsNoTracking().FirstOrDefaultAsync(i => i.ItemCode == bad));
    }

    [Fact]
    public async Task A_healthy_formula_workbook_is_imported_and_a_re_upload_is_skipped()
    {
        var workbook = FormulaWorkbook();

        var first = await PostAsync(FormulaRoute, workbook);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var firstReport = await first.Content.ReadFromJsonAsync<ImportReport>();
        Assert.NotNull(firstReport);
        Assert.Equal(2, firstReport!.Imported);
        Assert.Equal(0, firstReport.Skipped);
        Assert.Empty(firstReport.Failed);

        await using (var context = _fixture.CreateContext())
        {
            var headerItemId = await context.Items.AsNoTracking()
                .Where(i => i.ItemCode == HeaderCode)
                .Select(i => i.Id)
                .SingleAsync();

            var formula = await context.FormulaHeaders.AsNoTracking()
                .Include(f => f.Requirements)
                .SingleAsync(f => f.ItemId == headerItemId && f.Version == 1);

            Assert.Equal(2, formula.Requirements.Count);
        }

        var second = await PostAsync(FormulaRoute, workbook);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var secondReport = await second.Content.ReadFromJsonAsync<ImportReport>();
        Assert.NotNull(secondReport);
        Assert.Equal(0, secondReport!.Imported);
        Assert.Equal(2, secondReport.Skipped);
        Assert.Empty(secondReport.Failed);
    }

    [Fact]
    public async Task A_formula_workbook_with_one_invalid_row_returns_422_and_commits_nothing()
    {
        var workbook = Workbook(
            FormulaHeader,
            FormulaRow(HeaderCode, RawOneCode, "Raw One", "10", version: "2"),
            FormulaRow(HeaderCode, RawTwoCode, "Raw Two", "0", version: "2"));

        var response = await PostAsync(FormulaRoute, workbook);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var report = await response.Content.ReadFromJsonAsync<ImportReport>();
        Assert.NotNull(report);
        Assert.Equal(0, report!.Imported);
        Assert.Single(report.Failed);
    }

    [Fact]
    public async Task A_non_xlsx_upload_is_rejected_before_parsing()
    {
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(new byte[] { 1, 2, 3 }), "file", "raw-materials.csv");

        var response = await _client.PostAsync(RawMaterialsRoute, form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<HttpResponseMessage> PostAsync(string route, byte[] workbook)
    {
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(workbook);
        file.Headers.ContentType = new MediaTypeHeaderValue(XlsxContentType);
        form.Add(file, "file", "catalog-import.xlsx");

        return await _client.PostAsync(route, form);
    }

    private static string[] RawMaterialRow(string code, string buffer = "5")
        => new[] { code, "Imported Material", CategoryName, "KG", buffer };

    private static string[] FormulaRow(
        string formulaCode,
        string rawMaterialCode,
        string rawMaterialDescription,
        string quantity,
        string version = "1")
        => new[]
        {
            formulaCode, "Header Item", version, rawMaterialCode, rawMaterialDescription, quantity
        };

    private static byte[] FormulaWorkbook()
        => Workbook(
            FormulaHeader,
            FormulaRow(HeaderCode, RawOneCode, "Raw One", "10"),
            FormulaRow(HeaderCode, RawTwoCode, "Raw Two", "5"));

    private static byte[] Workbook(params string[][] rows)
    {
        using var stream = new MemoryStream();

        using (var workbook = new XLWorkbook())
        {
            var sheet = workbook.AddWorksheet("Sheet1");

            for (var rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < rows[rowIndex].Length; columnIndex++)
                    sheet.Cell(rowIndex + 1, columnIndex + 1).Value = rows[rowIndex][columnIndex];
            }

            workbook.SaveAs(stream);
        }

        return stream.ToArray();
    }

    private static string CreateToken()
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, "1"),
            new(ClaimTypes.Name, "Test User"),
            new("modules", "Catalog")
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
