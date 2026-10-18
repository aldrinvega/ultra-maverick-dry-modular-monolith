using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Ultramaverick.Api.AuthorizationTests;

public class WarehouseAuthorizationTests : IClassFixture<ApiFixture>
{
    private const string ReceiptsRoute = "/api/WarehouseReceiving/GetWarehouseReceiptsWithPagination";
    private const string AddWarehouseRoute = "/api/WarehouseReceiving/AddWarehouse";

    private readonly ApiFixture _fixture;

    public WarehouseAuthorizationTests(ApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task A_warehouse_endpoint_without_a_token_returns_401()
    {
        var client = _fixture.CreateClient();

        var response = await client.GetAsync(ReceiptsRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_token_without_the_warehouse_module_returns_403()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Identity", "Quality"));

        var response = await client.GetAsync(ReceiptsRoute);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task A_token_with_the_warehouse_module_is_allowed()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Warehouse"));

        var response = await client.GetAsync(ReceiptsRoute);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task An_add_warehouse_without_a_token_returns_401()
    {
        var client = _fixture.CreateClient();

        var response = await client.PostAsync(AddWarehouseRoute, AddWarehouseBody());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task An_add_warehouse_without_the_warehouse_module_returns_403()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Identity", "Quality"));

        var response = await client.PostAsync(AddWarehouseRoute, AddWarehouseBody());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task An_add_warehouse_with_the_warehouse_module_reaches_the_handler_and_returns_200()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Warehouse"));

        var response = await client.PostAsync(AddWarehouseRoute, AddWarehouseBody());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static StringContent AddWarehouseBody()
        => new($$"""
            {
              "code": "WH-{{Guid.NewGuid():N}}" ,
              "name": "Authorization Test Warehouse",
              "warehouseTypeId": null,
              "address": null
            }
            """, Encoding.UTF8, "application/json");
}
