using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Ultramaverick.Catalog.Persistence;

namespace Ultramaverick.Catalog.IntegrationTests;

/// <summary>
/// Creates a throwaway SQL Server database per test class, applies the Catalog EF
/// migrations, and drops it afterwards. The server comes from CATALOG_TEST_CONNECTION
/// when set (CI points this at its SQL Server service); otherwise local SQL Server Express.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    private const string DefaultServer =
        @"Server=.\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True";

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        var baseConnection = Environment.GetEnvironmentVariable("CATALOG_TEST_CONNECTION")
                             ?? DefaultServer;

        ConnectionString = new SqlConnectionStringBuilder(baseConnection)
        {
            InitialCatalog = $"UltramaverickCatalogTests_{Guid.NewGuid():N}"
        }.ConnectionString;

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }

    public CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new CatalogDbContext(options);
    }
}