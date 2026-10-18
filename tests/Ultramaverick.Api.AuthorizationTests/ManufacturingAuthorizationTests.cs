using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Ultramaverick.Api.AuthorizationTests;

public class ManufacturingAuthorizationTests : IClassFixture<ApiFixture>
{
    private const string AddTransformationRequestRoute = "/api/Manufacturing/AddTransformationRequest";

    private readonly ApiFixture _fixture;

    public ManufacturingAuthorizationTests(ApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task An_add_request_without_a_token_returns_401()
    {
        var client = _fixture.CreateClient();

        var response = await client.PostAsync(AddTransformationRequestRoute, AddRequestBody());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task An_add_request_without_the_manufacturing_module_returns_403()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Identity", "Orders"));

        var response = await client.PostAsync(AddTransformationRequestRoute, AddRequestBody());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task An_add_request_with_the_manufacturing_module_reaches_the_handler_and_returns_200()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Manufacturing"));

        var response = await client.PostAsync(AddTransformationRequestRoute, AddRequestBody());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private StringContent AddRequestBody()
        => new($$"""
            {
              "productionPlanId": {{_fixture.ProductionPlanId}},
              "warehouseId": 1,
              "lines": [
                { "itemId": 10, "requiredQuantity": 5 }
              ]
            }
            """, Encoding.UTF8, "application/json");
}
