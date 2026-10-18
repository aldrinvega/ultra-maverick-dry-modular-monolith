using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Catalog.Application;
using Ultramaverick.Catalog.Infrastructure;
using Ultramaverick.Catalog.Persistence;
using Ultramaverick.Identity.Application;
using Ultramaverick.Identity.Infrastructure;
using Ultramaverick.Identity.Persistence;
using Ultramaverick.Procurement.Application;
using Ultramaverick.Procurement.Infrastructure;
using Ultramaverick.Procurement.Persistence;
using Ultramaverick.Quality.Application;
using Ultramaverick.Quality.Infrastructure;
using Ultramaverick.Quality.Persistence;

namespace Ultramaverick.Quality.IntegrationTests;

/// <summary>
/// Creates a throwaway SQL Server database per test class and applies the Identity, Catalog,
/// Procurement and Quality EF migrations into it (all four schemas share one database). The
/// server comes from QUALITY_TEST_CONNECTION when set (CI points this at its SQL Server
/// service); otherwise local SQL Server Express.
///
/// It also composes the real service graph (every module's Add* extensions), so tests can
/// resolve the shared dispatcher, the per-module outbox stores and Quality's bridge.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    private const string DefaultServer =
        @"Server=.\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True";

    public string ConnectionString { get; private set; } = string.Empty;

    public ServiceProvider Services { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var baseConnection = Environment.GetEnvironmentVariable("QUALITY_TEST_CONNECTION")
                             ?? DefaultServer;

        ConnectionString = new SqlConnectionStringBuilder(baseConnection)
        {
            InitialCatalog = $"UltramaverickQualityTests_{Guid.NewGuid():N}"
        }.ConnectionString;

        await using (var quality = CreateContext())
            await quality.Database.MigrateAsync();

        await using (var identity = CreateIdentityContext())
            await identity.Database.MigrateAsync();

        await using (var catalog = CreateCatalogContext())
            await catalog.Database.MigrateAsync();

        await using (var procurement = CreateProcurementContext())
            await procurement.Database.MigrateAsync();

        Services = BuildServiceProvider();
    }

    public async Task DisposeAsync()
    {
        await Services.DisposeAsync();

        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }

    public QualityDbContext CreateContext(params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<QualityDbContext>()
            .UseSqlServer(ConnectionString)
            .AddInterceptors(interceptors)
            .Options;

        return new QualityDbContext(options);
    }

    public ProcurementDbContext CreateProcurementContext()
    {
        var options = new DbContextOptionsBuilder<ProcurementDbContext>()
            .UseSqlServer(ConnectionString)
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

    public IdentityDbContext CreateIdentityContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new IdentityDbContext(options);
    }

    public IServiceScope CreateScope() => Services.CreateScope();

    private ServiceProvider BuildServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = ConnectionString,
                ["Jwt:Issuer"] = "https://test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:Key"] = "test-signing-key-for-quality-integration-tests-0123456789",
                ["Jwt:AccessTokenMinutes"] = "60",
                ["Jwt:RefreshTokenDays"] = "7"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();

        services.AddIdentityApplication();
        services.AddIdentityPersistence(configuration);
        services.AddIdentityInfrastructure(configuration);

        services.AddCatalogApplication();
        services.AddCatalogPersistence(configuration);
        services.AddCatalogInfrastructure(configuration);

        services.AddProcurementApplication();
        services.AddProcurementPersistence(configuration);
        services.AddProcurementInfrastructure(configuration);

        services.AddQualityApplication();
        services.AddQualityPersistence(configuration);
        services.AddQualityInfrastructure(configuration);

        return services.BuildServiceProvider();
    }
}
