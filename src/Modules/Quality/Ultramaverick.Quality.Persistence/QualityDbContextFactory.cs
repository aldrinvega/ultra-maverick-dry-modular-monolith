using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ultramaverick.Quality.Persistence
{
    public sealed class QualityDbContextFactory : IDesignTimeDbContextFactory<QualityDbContext>
    {
        public QualityDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("QualityConnection")
                ?? "Server=.\\SQLEXPRESS;Database=ElixirDepotDry;Trusted_Connection=True;TrustServerCertificate=True";

            var options = new DbContextOptionsBuilder<QualityDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new QualityDbContext(options);
        }
    }
}
