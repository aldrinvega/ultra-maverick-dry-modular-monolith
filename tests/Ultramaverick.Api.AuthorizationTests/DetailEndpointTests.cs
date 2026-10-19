using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Ultramaverick.Api.AuthorizationTests;

public class DetailEndpointTests : IClassFixture<ApiFixture>
{
    private readonly ApiFixture _fixture;

    public DetailEndpointTests(ApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Order_detail_returns_the_order_that_was_created()
    {
        var client = Authorized("Orders");
        var body = Json($$"""
            {
              "customerId": {{_fixture.CustomerId}},
              "orderDate": null,
              "deliveryDate": null,
              "remarks": "detail-test",
              "lines": [
                { "itemId": 10, "uomId": 20, "quantity": 5, "formulaHeaderId": null }
              ]
            }
            """);

        var created = await client.PostAsync("/api/Ordering/AddOrder", body);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var id = await ReadInt(created);

        var detail = await client.GetAsync($"/api/Ordering/GetOrderById/{id}");
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);

        var root = await ReadJson(detail);
        Assert.Equal(id, root.GetProperty("id").GetInt32());
        Assert.Equal("detail-test", root.GetProperty("remarks").GetString());
    }

    [Fact]
    public async Task Purchase_order_detail_returns_the_order_that_was_created()
    {
        var client = Authorized("Procurement");
        var poNumber = "PO-" + Guid.NewGuid().ToString("N")[..10];
        var body = Json($$"""
            {
              "poNumber": "{{poNumber}}",
              "prNumber": null,
              "poDate": null,
              "prDate": null,
              "supplierId": {{_fixture.SupplierId}},
              "lines": [
                { "itemId": 10, "uomId": 20, "quantity": 5, "unitPrice": 12.5 }
              ]
            }
            """);

        var created = await client.PostAsync("/api/Ordering/AddPurchaseOrder", body);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var id = await ReadInt(created);

        var detail = await client.GetAsync($"/api/Ordering/GetPurchaseOrderById/{id}");
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);

        var root = await ReadJson(detail);
        Assert.Equal(id, root.GetProperty("id").GetInt32());
        Assert.Equal(poNumber, root.GetProperty("poNumber").GetString());
    }

    [Fact]
    public async Task Qc_receipt_detail_returns_the_seeded_receipt()
    {
        var id = await _fixture.SeedQcReceiptAsync();
        var client = Authorized("Quality");

        var detail = await client.GetAsync($"/api/Receiving/GetQcReceiptById/{id}");
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);

        var root = await ReadJson(detail);
        Assert.Equal(id, root.GetProperty("id").GetInt32());
        Assert.Equal(10, root.GetProperty("itemId").GetInt32());
    }

    [Fact]
    public async Task Warehouse_receipt_detail_returns_the_seeded_receipt()
    {
        var id = await _fixture.SeedWarehouseReceiptAsync();
        var client = Authorized("Warehouse");

        var detail = await client.GetAsync($"/api/WarehouseReceiving/GetWarehouseReceiptById/{id}");
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);

        var root = await ReadJson(detail);
        Assert.Equal(id, root.GetProperty("id").GetInt32());
        Assert.Equal(10, root.GetProperty("itemId").GetInt32());
    }

    private HttpClient Authorized(string module)
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken(module));
        return client;
    }

    private static StringContent Json(string json)
        => new(json, Encoding.UTF8, "application/json");

    private static async Task<int> ReadInt(HttpResponseMessage response)
        => int.Parse(await response.Content.ReadAsStringAsync());

    private static async Task<JsonElement> ReadJson(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.Clone();
    }
}
