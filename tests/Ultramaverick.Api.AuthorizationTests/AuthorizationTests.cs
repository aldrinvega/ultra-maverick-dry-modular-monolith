using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Ultramaverick.Api.AuthorizationTests;

public class AuthorizationTests : IClassFixture<ApiFixture>
{
    private readonly ApiFixture _fixture;

    public AuthorizationTests(ApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task A_protected_endpoint_without_a_token_returns_401()
    {
        var client = _fixture.CreateClient();

        var response = await client.GetAsync("/api/User/GetById/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_token_without_the_identity_module_returns_403()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Catalog", "Warehouse"));

        var response = await client.GetAsync("/api/User/GetById/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task A_token_with_the_identity_module_is_allowed()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Identity"));

        var response = await client.GetAsync("/api/User/GetById/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_is_anonymous_and_rejects_bad_credentials_with_400()
    {
        var client = _fixture.CreateClient();
        var body = new StringContent(
            """{ "userName": "admin", "password": "definitely-wrong" }""",
            Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/Login/authenticate", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
