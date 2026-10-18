using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Ultramaverick.Api.AuthorizationTests;

public class QcReceiptsAuthorizationTests : IClassFixture<ApiFixture>
{
    private const string QcReceiptsRoute = "/api/Receiving/GetQcReceiptsWithPagination";
    private const string RejectRoute = "/api/Receiving/Reject/1";

    private readonly ApiFixture _fixture;

    public QcReceiptsAuthorizationTests(ApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task A_quality_endpoint_without_a_token_returns_401()
    {
        var client = _fixture.CreateClient();

        var response = await client.GetAsync(QcReceiptsRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_token_without_the_quality_module_returns_403()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Identity", "Procurement"));

        var response = await client.GetAsync(QcReceiptsRoute);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task A_token_with_the_quality_module_is_allowed()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Quality"));

        var response = await client.GetAsync(QcReceiptsRoute);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task A_reject_without_a_token_returns_401()
    {
        var client = _fixture.CreateClient();

        var response = await client.PutAsync(RejectRoute, RejectBody());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_reject_without_the_quality_module_returns_403()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Identity", "Procurement"));

        var response = await client.PutAsync(RejectRoute, RejectBody());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task A_reject_with_the_quality_module_reaches_the_handler_and_returns_400()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Quality"));

        var response = await client.PutAsync(RejectRoute, RejectBody());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static StringContent RejectBody()
        => new("""{ "qcReceiptId": 1, "quantity": 1, "reasonId": null, "remarks": "damaged" }""",
            Encoding.UTF8, "application/json");
}
