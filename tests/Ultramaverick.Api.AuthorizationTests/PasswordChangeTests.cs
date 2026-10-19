using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Ultramaverick.Api.AuthorizationTests;

public class PasswordChangeTests : IClassFixture<ApiFixture>
{
    private const string AuthenticateRoute = "/api/Login/authenticate";
    private const string ChangePasswordRoute = "/api/Login/change-password";
    private const string ProtectedRoute = "/api/User/GetById/1";

    private readonly ApiFixture _fixture;

    public PasswordChangeTests(ApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task A_migrated_user_must_change_the_password_before_using_other_endpoints()
    {
        var client = _fixture.CreateClient();

        var login = await client.PostAsync(
            AuthenticateRoute,
            Json(new { userName = "migrated", password = "TempPass123!" }));

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var loginBody = ReadElement(await login.Content.ReadAsStringAsync());
        Assert.True(loginBody.GetProperty("mustChangePassword").GetBoolean());
        var temporaryToken = loginBody.GetProperty("accessToken").GetString()!;

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", temporaryToken);

        var blocked = await client.GetAsync(ProtectedRoute);
        Assert.Equal(HttpStatusCode.Forbidden, blocked.StatusCode);
        Assert.Contains("Password change required", await blocked.Content.ReadAsStringAsync());

        var changed = await client.PostAsync(
            ChangePasswordRoute,
            Json(new { currentPassword = "TempPass123!", newPassword = "NewPass123!" }));

        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);

        var changedBody = ReadElement(await changed.Content.ReadAsStringAsync());
        Assert.False(changedBody.GetProperty("mustChangePassword").GetBoolean());
        var newToken = changedBody.GetProperty("accessToken").GetString()!;

        var claims = new JwtSecurityTokenHandler().ReadJwtToken(newToken).Claims;
        Assert.DoesNotContain(claims, claim => claim.Type == "must_change_password");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", newToken);

        var wrongCurrent = await client.PostAsync(
            ChangePasswordRoute,
            Json(new { currentPassword = "wrong-password", newPassword = "Another123!" }));

        Assert.Equal(HttpStatusCode.Unauthorized, wrongCurrent.StatusCode);
    }

    private static JsonElement ReadElement(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static StringContent Json(object value)
        => new(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
}
