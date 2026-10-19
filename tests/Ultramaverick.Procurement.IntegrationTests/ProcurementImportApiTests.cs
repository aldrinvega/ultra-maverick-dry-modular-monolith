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
using Ultramaverick.Catalog.Domain.Entities;
using Ultramaverick.Procurement.Application.Models;

namespace Ultramaverick.Procurement.IntegrationTests;

/// <summary>
/// Boots the real API in-process against the throwaway database the fixture migrates, then
/// posts a generated legacy PO-summary workbook through the upload endpoint.
/// </summary>
public sealed class ProcurementImportApiTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private const string Issuer = "https://test-issuer";
    private const string Audience = "test-audience";
    private const string Key = "test-signing-key-for-authorization-tests-0123456789";
    private const string XlsxContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string Route = "/api/Procurement/UploadPOSummaryReport";

    private static readonly string ItemCode = $"IT{Guid.NewGuid():N}"[..10];
    private static readonly string SupplierName = $"Supplier {Guid.NewGuid():N}"[..20];

    private static readonly string[] Header =
    {
        "PR Number", "PR Date", "PO Number", "PO Date", "Item Code", "Item Description",
        "Ordered", "Delivered", "Billed", "UOM", "Unit Price", "Vendor Name"
    };

    private readonly DatabaseFixture _fixture;
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public ProcurementImportApiTests(DatabaseFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        await using (var catalog = _fixture.CreateCatalogContext())
        {
            var uom = Uom.Create("KG", "Kilogram", null);
            var category = ItemCategory.Create("Raw Materials", null);
            catalog.Uoms.Add(uom);
            catalog.ItemCategories.Add(category);
            await catalog.SaveChangesAsync();

            catalog.Items.Add(Item.Create(
                ItemCode, "Imported Item", category.Id, uom.Id, bufferLevel: null,
                isExpirable: false, createdByUserId: null));
            catalog.Suppliers.Add(Supplier.Create(
                $"SUP{Guid.NewGuid():N}"[..10], SupplierName, address: null, createdByUserId: null));
            await catalog.SaveChangesAsync();
        }

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

    [Fact]
    public async Task A_healthy_workbook_is_imported_and_a_re_upload_is_skipped()
    {
        var poNumber = $"PO-{Guid.NewGuid():N}"[..12];
        var workbook = Workbook(Header, DataRow(poNumber));

        var first = await PostAsync(workbook);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var firstReport = await first.Content.ReadFromJsonAsync<ImportReport>();
        Assert.NotNull(firstReport);
        Assert.Equal(1, firstReport.Imported);
        Assert.Equal(0, firstReport.Skipped);
        Assert.Empty(firstReport.Failed);

        var second = await PostAsync(workbook);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var secondReport = await second.Content.ReadFromJsonAsync<ImportReport>();
        Assert.NotNull(secondReport);
        Assert.Equal(0, secondReport.Imported);
        Assert.Equal(1, secondReport.Skipped);
        Assert.Empty(secondReport.Failed);
    }

    [Fact]
    public async Task A_workbook_with_one_invalid_row_returns_422_and_commits_nothing()
    {
        var poNumber = $"BAD-{Guid.NewGuid():N}"[..12];
        var workbook = Workbook(Header, DataRow(poNumber, ordered: "0"));

        var response = await PostAsync(workbook);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var report = await response.Content.ReadFromJsonAsync<ImportReport>();
        Assert.NotNull(report);
        Assert.Equal(0, report.Imported);
        Assert.Single(report.Failed);

        await using var context = _fixture.CreateContext();
        Assert.Null(await context.PurchaseOrders.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PoNumber == poNumber));
    }

    [Fact]
    public async Task A_non_xlsx_upload_is_rejected_before_parsing()
    {
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(new byte[] { 1, 2, 3 }), "file", "po-summary.csv");

        var response = await _client.PostAsync(Route, form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<HttpResponseMessage> PostAsync(byte[] workbook)
    {
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(workbook);
        file.Headers.ContentType = new MediaTypeHeaderValue(XlsxContentType);
        form.Add(file, "file", "po-summary.xlsx");

        return await _client.PostAsync(Route, form);
    }

    private static string[] DataRow(string poNumber, string ordered = "10")
        => new[]
        {
            "PR-1", string.Empty, poNumber, string.Empty, ItemCode, "Imported Item",
            ordered, string.Empty, string.Empty, "KG", "5", SupplierName
        };

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
            new("modules", "Procurement")
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
