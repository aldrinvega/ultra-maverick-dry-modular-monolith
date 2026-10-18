using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ultramaverick.Manufacturing.Persistence
{
    public sealed class ManufacturingDbContextFactory : IDesignTimeDbContextFactory<ManufacturingDbContext>
    {
        public ManufacturingDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("ManufacturingConnection")
                ?? "Server=.\\SQLEXPRESS;Database=ElixirDepotDry;Trusted_Connection=True;TrustServerCertificate=True";

            var options = new DbContextOptionsBuilder<ManufacturingDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new ManufacturingDbContext(options);
        }
    }
}
