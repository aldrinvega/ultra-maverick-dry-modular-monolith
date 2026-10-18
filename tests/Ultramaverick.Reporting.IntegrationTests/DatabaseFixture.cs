using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Reporting.Application;
using Ultramaverick.Reporting.Infrastructure;
using Ultramaverick.Reporting.Persistence;

namespace Ultramaverick.Reporting.IntegrationTests;

/// <summary>
/// Creates a throwaway SQL Server database per test class and applies the Reporting EF
/// migrations into it. The server comes from REPORTING_TEST_CONNECTION when set; otherwise local
/// SQL Server Express.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    private const string DefaultServer =
        @"Server=.\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True";

    public string ConnectionString { get; private set; } = string.Empty;

    public ServiceProvider Services { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var baseConnection = Environment.GetEnvironmentVariable("REPORTING_TEST_CONNECTION")
                             ?? DefaultServer;

        ConnectionString = new SqlConnectionStringBuilder(baseConnection)
        {
            InitialCatalog = $"UltramaverickReportingTests_{Guid.NewGuid():N}"
        }.ConnectionString;

        await using (var context = CreateContext())
            await context.Database.MigrateAsync();

        Services = BuildServiceProvider();
    }

    public async Task DisposeAsync()
    {
        await Services.DisposeAsync();

        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }

    public ReportingDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ReportingDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new ReportingDbContext(options);
    }

    public IServiceScope CreateScope() => Services.CreateScope();

    private ServiceProvider BuildServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = ConnectionString
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();

        services.AddReportingApplication();
        services.AddReportingPersistence(configuration);
        services.AddReportingInfrastructure(configuration);

        return services.BuildServiceProvider();
    }
}
