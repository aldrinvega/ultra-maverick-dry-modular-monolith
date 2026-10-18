using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Manufacturing.Application;
using Ultramaverick.Manufacturing.Infrastructure;
using Ultramaverick.Manufacturing.Persistence;

namespace Ultramaverick.Manufacturing.IntegrationTests;

/// <summary>
/// Creates a throwaway SQL Server database per test class and applies the Manufacturing EF
/// migrations into it. The server comes from MANUFACTURING_TEST_CONNECTION when set (CI points
/// this at its SQL Server service); otherwise local SQL Server Express.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    private const string DefaultServer =
        @"Server=.\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True";

    public string ConnectionString { get; private set; } = string.Empty;

    public ServiceProvider Services { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var baseConnection = Environment.GetEnvironmentVariable("MANUFACTURING_TEST_CONNECTION")
                             ?? DefaultServer;

        ConnectionString = new SqlConnectionStringBuilder(baseConnection)
        {
            InitialCatalog = $"UltramaverickManufacturingTests_{Guid.NewGuid():N}"
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

    public ManufacturingDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ManufacturingDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new ManufacturingDbContext(options);
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

        services.AddManufacturingApplication();
        services.AddManufacturingPersistence(configuration);
        services.AddManufacturingInfrastructure(configuration);

        return services.BuildServiceProvider();
    }
}
