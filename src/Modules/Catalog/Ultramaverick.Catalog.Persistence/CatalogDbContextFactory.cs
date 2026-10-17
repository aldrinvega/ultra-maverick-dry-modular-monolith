using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ultramaverick.Catalog.Persistence
{
    public sealed class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
    {
        public CatalogDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("CatalogConnection")
                ?? "Server=.\\SQLEXPRESS;Database=ElixirDepotDry;Trusted_Connection=True;TrustServerCertificate=True";

            var options = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new CatalogDbContext(options);
        }
    }
}