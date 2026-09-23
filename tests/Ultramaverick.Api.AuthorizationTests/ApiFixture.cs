using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Ultramaverick.Identity.Domain.Entities;
using Ultramaverick.Identity.Domain.ValueObjects;
using Ultramaverick.Identity.Persistence;

namespace Ultramaverick.Api.AuthorizationTests;

/// <summary>
/// Boots the real API in-process against a throwaway database, and issues test JWTs
/// carrying the modules claims the authorization policies read.
/// </summary>
public sealed class ApiFixture : IAsyncLifetime
{
    public const string Issuer = "https://test-issuer";
    public const string Audience = "test-audience";
    public const string Key = "test-signing-key-for-authorization-tests-0123456789";

    private const string DefaultServer =
        @"Server=.\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True";

    private WebApplicationFactory<Program> _factory = null!;
    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        var baseConnection = Environment.GetEnvironmentVariable("IDENTITY_TEST_CONNECTION")
                             ?? DefaultServer;

        ConnectionString = new SqlConnectionStringBuilder(baseConnection)
        {
            InitialCatalog = $"UltramaverickAuthorizationTests_{Guid.NewGuid():N}"
        }.ConnectionString;

        await using (var context = CreateContext())
        {
            await context.Database.MigrateAsync();
            await SeedAsync(context);
        }

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Default", ConnectionString);
            builder.UseSetting("Jwt:Issuer", Issuer);
            builder.UseSetting("Jwt:Audience", Audience);
            builder.UseSetting("Jwt:Key", Key);
            builder.UseSetting("Jwt:AccessTokenMinutes", "60");
            builder.UseSetting("Jwt:RefreshTokenDays", "7");
        });
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null) _factory.Dispose();

        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }

    public HttpClient CreateClient() => _factory.CreateClient();

    /// <summary>Builds a signed access token containing one modules claim per supplied module.</summary>
    public string CreateToken(params string[] modules)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, "1"),
            new(ClaimTypes.Name, "Test User")
        };

        foreach (var module in modules)
            claims.Add(new Claim("modules", module));

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private IdentityDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new IdentityDbContext(options);
    }

    private static async Task SeedAsync(IdentityDbContext context)
    {
        var role = Role.Create("Administrator", null);
        var department = Department.Create("Warehouse");
        context.Roles.Add(role);
        context.Departments.Add(department);
        await context.SaveChangesAsync();

        // First user gets id 1, which the tests request.
        var user = User.Create(
            "System Administrator",
            "admin",
            PasswordHash.FromEncoded("PBKDF2;SHA256;210000;AAAAAAAAAAAAAAAAAAAAAA==;AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="),
            role.Id,
            department.Id,
            null);

        context.Users.Add(user);
        await context.SaveChangesAsync();
    }
}
