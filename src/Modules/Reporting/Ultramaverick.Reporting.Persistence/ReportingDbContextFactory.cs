using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ultramaverick.Reporting.Persistence
{
    public sealed class ReportingDbContextFactory : IDesignTimeDbContextFactory<ReportingDbContext>
    {
        public ReportingDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("ReportingConnection")
                ?? "Server=.\\SQLEXPRESS;Database=ElixirDepotDry;Trusted_Connection=True;TrustServerCertificate=True";

            var options = new DbContextOptionsBuilder<ReportingDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new ReportingDbContext(options);
        }
    }
}
