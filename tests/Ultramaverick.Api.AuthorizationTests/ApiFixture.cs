using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Ultramaverick.Catalog.Domain.Entities;
using Ultramaverick.Catalog.Persistence;
using Ultramaverick.Identity.Domain.Entities;
using Ultramaverick.Identity.Domain.ValueObjects;
using Ultramaverick.Identity.Persistence;
using Ultramaverick.Orders.Persistence;
using Ultramaverick.Manufacturing.Persistence;
using Ultramaverick.Procurement.Persistence;
using Ultramaverick.Quality.Persistence;
using Ultramaverick.Laboratory.Persistence;
using Ultramaverick.Warehouse.Persistence;

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

    /// <summary>Id of the active Catalog customer seeded for the Orders create-order tests.</summary>
    public int CustomerId { get; private set; }

    /// <summary>Id of the production plan seeded for the Manufacturing create-request tests.</summary>
    public int ProductionPlanId { get; private set; }

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

        // Catalog owns its own schema, so apply its migrations too.
        await using (var catalogContext = CreateCatalogContext())
        {
            await catalogContext.Database.MigrateAsync();
            CustomerId = await SeedCustomerAsync(catalogContext);
        }

        await using (var ordersContext = CreateOrdersContext())
        {
            await ordersContext.Database.MigrateAsync();
        }

        await using (var procurementContext = CreateProcurementContext())
        {
            await procurementContext.Database.MigrateAsync();
        }

        await using (var qualityContext = CreateQualityContext())
        {
            await qualityContext.Database.MigrateAsync();
        }

        await using (var laboratoryContext = CreateLaboratoryContext())
        {
            await laboratoryContext.Database.MigrateAsync();
        }

        await using (var warehouseContext = CreateWarehouseContext())
        {
            await warehouseContext.Database.MigrateAsync();
        }

        await using (var manufacturingContext = CreateManufacturingContext())
        {
            await manufacturingContext.Database.MigrateAsync();
            ProductionPlanId = await SeedProductionPlanAsync(manufacturingContext);
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

    private CatalogDbContext CreateCatalogContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new CatalogDbContext(options);
    }

    private ProcurementDbContext CreateProcurementContext()
    {
        var options = new DbContextOptionsBuilder<ProcurementDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new ProcurementDbContext(options);
    }

    private QualityDbContext CreateQualityContext()
    {
        var options = new DbContextOptionsBuilder<QualityDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new QualityDbContext(options);
    }

    private LaboratoryDbContext CreateLaboratoryContext()
    {
        var options = new DbContextOptionsBuilder<LaboratoryDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new LaboratoryDbContext(options);
    }

    private WarehouseDbContext CreateWarehouseContext()
    {
        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new WarehouseDbContext(options);
    }

    private OrdersDbContext CreateOrdersContext()
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new OrdersDbContext(options);
    }

    private ManufacturingDbContext CreateManufacturingContext()
    {
        var options = new DbContextOptionsBuilder<ManufacturingDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new ManufacturingDbContext(options);
    }

    /// <summary>
    /// Seeds one production plan so Manufacturing's create-request handler has a valid target.
    /// ProductionPlan is map-only this wave, so the row is inserted with SQL.
    /// </summary>
    private static async Task<int> SeedProductionPlanAsync(ManufacturingDbContext context)
    {
        await context.Database.ExecuteSqlAsync($"""
            INSERT INTO [Manufacturing].[ProductionPlans]
                ([ItemId], [UomId], [Quantity], [Status], [IsApproved], [IsPrepared], [IsMixed], [IsActive], [CreatedAtUtc])
            VALUES (10, 20, 100, 'Pending', 0, 0, 0, 1, SYSUTCDATETIME());
            """);

        return await context.ProductionPlans
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Select(x => x.Id)
            .FirstAsync();
    }

    /// <summary>Seeds one active customer so Orders' create-order handler has a valid target.</summary>
    private static async Task<int> SeedCustomerAsync(CatalogDbContext context)
    {
        var customer = Customer.Create(
            $"C{Guid.NewGuid():N}"[..12], "Authorization Test Customer",
            null, null, null, null, null, null, null, null, null, null);

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        return customer.Id;
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
