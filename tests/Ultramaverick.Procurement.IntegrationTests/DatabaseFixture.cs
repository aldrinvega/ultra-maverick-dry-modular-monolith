using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ultramaverick.Catalog.Persistence;
using Ultramaverick.Procurement.Persistence;

namespace Ultramaverick.Procurement.IntegrationTests;

/// <summary>
/// Creates a throwaway SQL Server database per test class, applies the Procurement EF
/// migrations, and drops it afterwards. The server comes from PROCUREMENT_TEST_CONNECTION
/// when set (CI points this at its SQL Server service); otherwise local SQL Server Express.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    private const string DefaultServer =
        @"Server=.\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True";

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        var baseConnection = Environment.GetEnvironmentVariable("PROCUREMENT_TEST_CONNECTION")
                             ?? DefaultServer;

        ConnectionString = new SqlConnectionStringBuilder(baseConnection)
        {
            InitialCatalog = $"UltramaverickProcurementTests_{Guid.NewGuid():N}"
        }.ConnectionString;

        await using (var context = CreateContext())
            await context.Database.MigrateAsync();

        await using (var catalogContext = CreateCatalogContext())
            await catalogContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }

    public ProcurementDbContext CreateContext(params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<ProcurementDbContext>()
            .UseSqlServer(ConnectionString)
            .AddInterceptors(interceptors)
            .Options;

        return new ProcurementDbContext(options);
    }

    public CatalogDbContext CreateCatalogContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new CatalogDbContext(options);
    }
}
