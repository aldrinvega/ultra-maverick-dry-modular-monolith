using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Ultramaverick.Api.AuthorizationTests;

public class ReferenceDataEndpointTests : IClassFixture<ApiFixture>
{
    private readonly ApiFixture _fixture;

    public ReferenceDataEndpointTests(ApiFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("/api/RawMaterial/GetAllItemCategory")]
    [InlineData("/api/Lot/GetAllLotCategories")]
    [InlineData("/api/Customer/GetAllFarms")]
    [InlineData("/api/Transaction/GetAllActiveTransactionName")]
    [InlineData("/api/LabTestMasterList/GetAllSampleTypes")]
    public async Task Reference_lists_are_served_over_http(string route)
    {
        var client = Authorized();

        var response = await client.GetAsync(route);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(document.RootElement.TryGetProperty("items", out _));
    }

    [Fact]
    public async Task An_item_category_can_be_created_and_deactivated()
    {
        var client = Authorized();

        var created = await client.PostAsync(
            "/api/RawMaterial/AddNewItemCategories",
            Json(new { name = "Endpoint Test Category", code = (string?)null }));

        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var id = int.Parse(await created.Content.ReadAsStringAsync());

        var deactivated = await client.PutAsync(
            $"/api/RawMaterial/InActiveItemCategory/{id}", content: null);

        Assert.Equal(HttpStatusCode.NoContent, deactivated.StatusCode);
    }

    private HttpClient Authorized()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _fixture.CreateToken("Catalog"));
        return client;
    }

    private static StringContent Json(object value)
        => new(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
}
