using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Ultramaverick.Api.AuthorizationTests;

public class LabTestsAuthorizationTests : IClassFixture<ApiFixture>
{
    private const string AddRoute = "/api/LaboratoryTest/AddLabTestRequest";
    private const string RecordRoute = "/api/LaboratoryTest/RecordResult/1";

    private readonly ApiFixture _fixture;

    public LabTestsAuthorizationTests(ApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task A_laboratory_endpoint_without_a_token_returns_401()
    {
        var client = _fixture.CreateClient();

        var response = await client.PostAsync(AddRoute, AddBody(quantity: 5));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_token_without_the_laboratory_module_returns_403()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Identity", "Quality"));

        var response = await client.PostAsync(AddRoute, AddBody(quantity: 5));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task A_token_with_the_laboratory_module_reaches_the_handler_and_returns_200()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Laboratory"));

        var response = await client.PostAsync(AddRoute, AddBody(quantity: 5));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task An_invalid_quantity_is_rejected_with_400()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Laboratory"));

        var response = await client.PostAsync(AddRoute, AddBody(quantity: 0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task A_record_without_a_token_returns_401()
    {
        var client = _fixture.CreateClient();

        var response = await client.PutAsync(RecordRoute, RecordBody(labReceiveRequestId: 2));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_record_with_a_mismatched_route_id_returns_400()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Laboratory"));

        var response = await client.PutAsync(RecordRoute, RecordBody(labReceiveRequestId: 2));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static StringContent AddBody(int quantity)
        => new($$"""
            {
              "warehouseReceiptId": null,
              "batchNo": "LOT-1",
              "quantity": {{quantity}},
              "testType": "Micro",
              "dateRequested": null,
              "dateNeeded": null,
              "analysisIds": null,
              "parameterIds": null,
              "productConditionIds": null,
              "sampleTypeIds": null,
              "swabTypeIds": null
            }
            """, Encoding.UTF8, "application/json");

    private static StringContent RecordBody(int labReceiveRequestId)
        => new($$"""
            {
              "labReceiveRequestId": {{labReceiveRequestId}},
              "analyzedAt": "2026-01-02T03:04:05Z",
              "result": "Passed"
            }
            """, Encoding.UTF8, "application/json");
}
