using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Ultramaverick.Api.AuthorizationTests;

public class OrdersAuthorizationTests : IClassFixture<ApiFixture>
{
    private const string AddOrderRoute = "/api/Ordering/AddOrder";

    private readonly ApiFixture _fixture;

    public OrdersAuthorizationTests(ApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task An_add_order_without_a_token_returns_401()
    {
        var client = _fixture.CreateClient();

        var response = await client.PostAsync(AddOrderRoute, AddOrderBody());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task An_add_order_without_the_orders_module_returns_403()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Identity", "Quality"));

        var response = await client.PostAsync(AddOrderRoute, AddOrderBody());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task An_add_order_with_the_orders_module_reaches_the_handler_and_returns_200()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Orders"));

        var response = await client.PostAsync(AddOrderRoute, AddOrderBody());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private StringContent AddOrderBody()
        => new($$"""
            {
              "customerId": {{_fixture.CustomerId}},
              "orderDate": null,
              "deliveryDate": null,
              "remarks": null,
              "lines": [
                { "itemId": 10, "uomId": 20, "quantity": 5, "formulaHeaderId": null }
              ]
            }
            """, Encoding.UTF8, "application/json");
}
