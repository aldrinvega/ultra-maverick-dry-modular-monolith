using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Ultramaverick.Identity.Persistence;

namespace Ultramaverick.Identity.IntegrationTests;

/// <summary>
/// Creates a throwaway SQL Server database per test class, applies the EF migrations,
/// and drops it afterwards. The server is taken from IDENTITY_TEST_CONNECTION when set
/// (CI points this at its SQL Server service); otherwise local SQL Server Express.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    private const string DefaultServer =
        @"Server=.\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True";

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        var baseConnection = Environment.GetEnvironmentVariable("IDENTITY_TEST_CONNECTION")
                             ?? DefaultServer;

        ConnectionString = new SqlConnectionStringBuilder(baseConnection)
        {
            InitialCatalog = $"UltramaverickIdentityTests_{Guid.NewGuid():N}"
        }.ConnectionString;

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }

    public IdentityDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new IdentityDbContext(options);
    }
}
