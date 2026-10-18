using System.Net;
using System.Net.Http.Headers;

namespace Ultramaverick.Api.AuthorizationTests;

public class ReportingAuthorizationTests : IClassFixture<ApiFixture>
{
    private const string InventoryPositionRoute = "/api/Reports/InventoryPosition";

    private readonly ApiFixture _fixture;

    public ReportingAuthorizationTests(ApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Reading_inventory_positions_without_a_token_returns_401()
    {
        var client = _fixture.CreateClient();

        var response = await client.GetAsync(InventoryPositionRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Reading_inventory_positions_without_the_reporting_module_returns_403()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Identity", "Orders"));

        var response = await client.GetAsync(InventoryPositionRoute);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Reading_inventory_positions_with_the_reporting_module_returns_200()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Reporting"));

        var response = await client.GetAsync(InventoryPositionRoute);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
