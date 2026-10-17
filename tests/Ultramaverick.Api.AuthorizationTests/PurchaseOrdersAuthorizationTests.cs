using System.Net;
using System.Net.Http.Headers;

namespace Ultramaverick.Api.AuthorizationTests;

public class PurchaseOrdersAuthorizationTests : IClassFixture<ApiFixture>
{
    private const string PurchaseOrdersRoute = "/api/Ordering/GetPurchaseOrders";

    private readonly ApiFixture _fixture;

    public PurchaseOrdersAuthorizationTests(ApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task A_procurement_endpoint_without_a_token_returns_401()
    {
        var client = _fixture.CreateClient();

        var response = await client.GetAsync(PurchaseOrdersRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_token_without_the_procurement_module_returns_403()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Identity", "Catalog"));

        var response = await client.GetAsync(PurchaseOrdersRoute);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task A_token_with_the_procurement_module_is_allowed()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Procurement"));

        var response = await client.GetAsync(PurchaseOrdersRoute);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
